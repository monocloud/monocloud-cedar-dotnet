/*
 * Copyright Cedar Contributors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
use cedar_policy::entities_errors::EntitiesError;
#[cfg(feature = "partial-eval")]
use cedar_policy::ffi::is_authorized_partial_json_str;
use cedar_policy::ffi::{
    schema_to_json, schema_to_text, PolicySet as PolicySetFFI, Schema as FFISchema,
    SchemaToJsonAnswer, SchemaToTextAnswer,
};
use cedar_policy::{
    ffi::{is_authorized_json_str, validate_json_str},
    Entities, EntityId, EntityTypeName, EntityUid, Policy, PolicySet, Schema, Template,
};
use cedar_policy_formatter::{policies_str_to_pretty, Config};
use serde::{Deserialize, Serialize};
use serde_json::{from_str, Value};
use std::{collections::BTreeMap, error::Error, panic, str::FromStr};

use crate::answer::Answer;
use crate::helpers::validate_with_level_json_str;

type Result<T> = std::result::Result<T, Box<dyn Error>>;

const V0_AUTH_OP: &str = "AuthorizationOperation";
#[cfg(feature = "partial-eval")]
const V0_AUTH_PARTIAL_OP: &str = "AuthorizationPartialOperation";
const V0_VALIDATE_OP: &str = "ValidateOperation";
const V0_VALIDATE_LEVEL_OP: &str = "ValidateWithLevelOperation";
const V0_VALIDATE_ENTITIES: &str = "ValidateEntities";

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
struct ParsedPolicySet {
    static_policies: Vec<ParsedPolicy>,
    templates: Vec<ParsedPolicy>,
}

#[derive(Debug, Serialize)]
struct ParsedPolicy {
    id: String,
    text: String,
}

pub fn call_cedar(call: &str, input: &str) -> String {
    let result = match call {
        V0_AUTH_OP => is_authorized_json_str(input),
        #[cfg(feature = "partial-eval")]
        V0_AUTH_PARTIAL_OP => is_authorized_partial_json_str(input),
        V0_VALIDATE_OP => validate_json_str(input),
        V0_VALIDATE_ENTITIES => json_validate_entities(input),
        V0_VALIDATE_LEVEL_OP => validate_with_level_json_str(input),
        _ => {
            let ires = Answer::fail_internally(format!("unsupported operation: {}", call));
            serde_json::to_string(&ires)
        }
    };
    result.unwrap_or_else(|err| {
        panic!("failed to handle call {call} with input {input}\nError: {err}")
    })
}

#[derive(Serialize, Deserialize)]
struct ValidateEntityCall {
    schema: Value,
    entities: Value,
}

pub fn json_validate_entities(input: &str) -> serde_json::Result<String> {
    let ans = validate_entities(input)?;
    serde_json::to_string(&ans)
}

/// public string-based JSON interface to be invoked by FFIs. Takes in a `ValidateEntityCall` and (if successful)
/// returns unit value () which is null value when serialized to json.
pub fn validate_entities(input: &str) -> serde_json::Result<Answer> {
    let validate_entity_call = from_str::<ValidateEntityCall>(input)?;
    let schema = match validate_entity_call.schema {
        Value::String(cedarschema_str) => match Schema::from_cedarschema_str(&cedarschema_str) {
            Ok(s) => s.0,
            Err(e) => return Ok(Answer::fail_bad_request(vec![e.to_string()])),
        },
        cedarschema_json_obj => match Schema::from_json_value(cedarschema_json_obj) {
            Ok(s) => s,
            Err(e) => return Ok(Answer::fail_bad_request(vec![e.to_string()])),
        },
    };

    match Entities::from_json_value(validate_entity_call.entities, Some(&schema)) {
        Err(error) => {
            let err_message = match error {
                EntitiesError::Serialization(err) => err.to_string(),
                EntitiesError::Deserialization(err) => err.to_string(),
                EntitiesError::Duplicate(err) => err.to_string(),
                EntitiesError::TransitiveClosureError(err) => err.to_string(),
                EntitiesError::InvalidEntity(err) => err.to_string(),
            };
            Ok(Answer::fail_bad_request(vec![err_message]))
        }
        Ok(_entities) => Ok(Answer::Success {
            result: "null".to_string(),
        }),
    }
}

/// Public string-based JSON interface to parse a schema in Cedar's JSON format
pub fn parse_json_schema(schema: &str) -> Result<String> {
    Schema::from_json_str(schema)?;
    Ok("success".to_string())
}

/// public string-based JSON interface to parse a schema in Cedar's cedar-readable format
pub fn parse_cedar_schema(schema: &str) -> Result<String> {
    let (_schema, warnings) = Schema::from_cedarschema_str(schema)?;
    for _warning in warnings {}
    Ok("success".to_string())
}

pub fn parse_policy(policy: &str) -> Result<String> {
    Ok(Policy::from_str(policy)?.to_string())
}

pub fn policy_set_to_json(policy_set: &str) -> Result<String> {
    let policy_set_ffi: PolicySetFFI = serde_json::from_str(policy_set)?;
    let policy_set = policy_set_ffi
        .parse()
        .map_err(|err| format!("Error parsing policy set: {:?}", err))?;
    Ok(serde_json::to_string(&policy_set.to_json().unwrap())?)
}

pub fn parse_policies(policies: &str) -> Result<String> {
    let policy_set = PolicySet::from_str(policies)?;
    let static_policies = policy_set
        .policies()
        .map(|policy| ParsedPolicy {
            id: policy.id().to_string(),
            text: policy.to_string(),
        })
        .collect();
    let templates = policy_set
        .templates()
        .map(|template| ParsedPolicy {
            id: template.id().to_string(),
            text: template.to_string(),
        })
        .collect();
    Ok(serde_json::to_string(&ParsedPolicySet {
        static_policies,
        templates,
    })?)
}

pub fn get_policy_annotations(policy: &str) -> Result<String> {
    let policy = Policy::from_str(policy)?;
    let annotations: BTreeMap<&str, &str> = policy.annotations().collect();
    Ok(serde_json::to_string(&annotations)?)
}

pub fn get_template_annotations(template: &str) -> Result<String> {
    let template = Template::from_str(template)?;
    let annotations: BTreeMap<&str, &str> = template.annotations().collect();
    Ok(serde_json::to_string(&annotations)?)
}

pub fn parse_policy_template(template: &str) -> Result<String> {
    Ok(Template::from_str(template)?.to_string())
}

pub fn policy_to_json(policy: &str) -> Result<String> {
    let policy = Policy::from_str(policy)?;
    Ok(serde_json::to_string(&policy.to_json().unwrap())?)
}

pub fn policy_effect(policy: &str) -> Result<String> {
    Ok(Policy::from_str(policy)?.effect().to_string())
}

pub fn template_effect(template: &str) -> Result<String> {
    Ok(Template::from_str(template)?.effect().to_string())
}

pub fn policy_from_json(policy_json: &str) -> Result<String> {
    let policy_json_value: Value = serde_json::from_str(policy_json)?;
    Ok(Policy::from_json(None, policy_json_value)?.to_string())
}

pub fn entity_identifier_repr(id: &str) -> Result<String> {
    let id = match EntityId::from_str(id) {
        Ok(id) => id,
        Err(empty) => match empty {},
    };
    Ok(id.escaped().to_string())
}

pub fn parse_entity_type_name(src: &str) -> Result<String> {
    match EntityTypeName::from_str(src) {
        Ok(entity_type_name) => Ok(entity_type_name.to_string()),
        Err(_) => Ok("null".to_string()),
    }
}

pub fn entity_type_name_repr(src: &str) -> Result<String> {
    Ok(EntityTypeName::from_str(src)?.to_string())
}

pub fn parse_entity_uid(src: &str) -> Result<String> {
    match EntityUid::from_str(src) {
        Ok(entity_uid) => Ok(entity_uid.to_string()),
        Err(_) => Ok("null".to_string()),
    }
}

pub fn euid_repr(type_name: &str, id: &str) -> Result<String> {
    let type_name = EntityTypeName::from_str(type_name)?;
    let id = match EntityId::from_str(id) {
        Ok(id) => id,
        Err(empty) => match empty {},
    };
    Ok(EntityUid::from_type_name_and_id(type_name, id).to_string())
}

pub fn policies_to_pretty(policies: &str, config: Option<Config>) -> Result<String> {
    Ok(policies_str_to_pretty(
        policies,
        &config.unwrap_or_default(),
    )?)
}

pub fn json_schema_to_cedar(json_schema: &str) -> Result<String> {
    let schema: FFISchema = serde_json::from_str(json_schema)?;
    match schema_to_text(schema) {
        SchemaToTextAnswer::Success { text, warnings: _ } => Ok(text),
        SchemaToTextAnswer::Failure { errors } => {
            let joined_errors = errors
                .iter()
                .map(|e| e.message.clone())
                .collect::<Vec<_>>()
                .join("; ");
            Err(joined_errors.into())
        }
    }
}

pub fn cedar_schema_to_json(cedar_schema: &str) -> Result<String> {
    let cedar_schema = FFISchema::Cedar(cedar_schema.into());
    match schema_to_json(cedar_schema) {
        SchemaToJsonAnswer::Success { json, warnings: _ } => {
            Ok(serde_json::to_string_pretty(&json)?)
        }
        SchemaToJsonAnswer::Failure { errors } => {
            let joined_errors = errors
                .iter()
                .map(|e| e.message.clone())
                .collect::<Vec<_>>()
                .join("; ");
            Err(joined_errors.into())
        }
    }
}
