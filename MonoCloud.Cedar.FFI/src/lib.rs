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

#![allow(unsafe_code)]
#![allow(clippy::missing_safety_doc)]
mod answer;
mod helpers;
mod interface;
mod tests;
pub use interface::*;

use std::ffi::{CStr, CString};
use std::os::raw::{c_char, c_long, c_ulong};

use answer::Answer;
use cedar_policy_formatter::Config;

unsafe fn read_str<'a>(value: *const c_char) -> &'a str {
    CStr::from_ptr(value)
        .to_str()
        .expect("FFI string arguments must be valid UTF-8")
}

fn into_raw(value: String) -> *mut c_char {
    CString::new(value)
        .expect("FFI string results must not contain interior null bytes")
        .into_raw()
}

fn answer(result: std::result::Result<String, Box<dyn std::error::Error>>) -> *mut c_char {
    let payload = match result {
        Ok(result) => Answer::Success { result },
        Err(err) => Answer::fail_internally(err.to_string()),
    };
    into_raw(serde_json::to_string(&payload).expect("failed to serialize FFI answer"))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_free_string(value: *mut c_char) {
    if !value.is_null() {
        drop(CString::from_raw(value));
    }
}

#[no_mangle]
pub extern "C" fn cedar_version() -> *mut c_char {
    into_raw("4.0".to_string())
}

#[no_mangle]
pub unsafe extern "C" fn cedar_call(operation: *const c_char, input: *const c_char) -> *mut c_char {
    into_raw(call_cedar(read_str(operation), read_str(input)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_policy(policy: *const c_char) -> *mut c_char {
    answer(parse_policy(read_str(policy)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_policy_to_json(policy: *const c_char) -> *mut c_char {
    answer(policy_to_json(read_str(policy)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_policy_from_json(policy_json: *const c_char) -> *mut c_char {
    answer(policy_from_json(read_str(policy_json)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_policy_set_to_json(policy_set: *const c_char) -> *mut c_char {
    answer(policy_set_to_json(read_str(policy_set)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_policies(policies: *const c_char) -> *mut c_char {
    answer(parse_policies(read_str(policies)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_get_policy_annotations(policy: *const c_char) -> *mut c_char {
    answer(get_policy_annotations(read_str(policy)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_get_template_annotations(template: *const c_char) -> *mut c_char {
    answer(get_template_annotations(read_str(template)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_policy_template(template: *const c_char) -> *mut c_char {
    answer(parse_policy_template(read_str(template)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_policy_effect(policy: *const c_char) -> *mut c_char {
    answer(policy_effect(read_str(policy)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_template_effect(template: *const c_char) -> *mut c_char {
    answer(template_effect(read_str(template)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_entity_identifier_repr(id: *const c_char) -> *mut c_char {
    answer(entity_identifier_repr(read_str(id)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_entity_type_name(type_name: *const c_char) -> *mut c_char {
    answer(parse_entity_type_name(read_str(type_name)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_entity_type_name_repr(type_name: *const c_char) -> *mut c_char {
    answer(entity_type_name_repr(read_str(type_name)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_entity_uid(entity_uid: *const c_char) -> *mut c_char {
    answer(parse_entity_uid(read_str(entity_uid)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_euid_repr(
    type_name: *const c_char,
    id: *const c_char,
) -> *mut c_char {
    answer(euid_repr(read_str(type_name), read_str(id)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_json_schema(schema: *const c_char) -> *mut c_char {
    answer(parse_json_schema(read_str(schema)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_parse_cedar_schema(schema: *const c_char) -> *mut c_char {
    answer(parse_cedar_schema(read_str(schema)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_policies_str_to_pretty(policies: *const c_char) -> *mut c_char {
    answer(policies_to_pretty(read_str(policies), None))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_policies_str_to_pretty_with_config(
    policies: *const c_char,
    line_width: c_ulong,
    indent_width: c_long,
) -> *mut c_char {
    answer(policies_to_pretty(
        read_str(policies),
        Some(Config {
            line_width: line_width as usize,
            indent_width: indent_width as isize,
        }),
    ))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_json_to_cedar_schema(json_schema: *const c_char) -> *mut c_char {
    answer(json_schema_to_cedar(read_str(json_schema)))
}

#[no_mangle]
pub unsafe extern "C" fn cedar_cedar_to_json_schema(cedar_schema: *const c_char) -> *mut c_char {
    answer(cedar_schema_to_json(read_str(cedar_schema)))
}
