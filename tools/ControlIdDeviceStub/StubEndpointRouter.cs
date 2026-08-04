using System.Text;
using System.Text.Json.Nodes;

internal static class StubEndpointRouter
{
    public static IResult Route(HttpRequest request, string path, JsonNode? body, StubRuntimeState runtime)
    {
        var state = runtime.Device;

        return path switch
        {
            "/login.fcgi" => Results.Json(new { session = state.Session }),
            "/logout.fcgi" => Results.Json(new { }),
            "/session_is_valid.fcgi" => Results.Json(new { session_is_valid = true }),
            "/user_hash_password.fcgi" => Results.Json(new { password = "stub-hash", salt = "stub-salt" }),

            "/load_objects.fcgi" => Results.Json(state.LoadObjects(body)),
            "/create_objects.fcgi" => Results.Json(state.CreateObjects(body)),
            "/create_or_modify_objects.fcgi" => Results.Json(state.CreateOrModifyObjects(body)),
            "/modify_objects.fcgi" => Results.Json(state.ModifyObjects(body)),
            "/destroy_objects.fcgi" => Results.Json(state.DestroyObjects(body)),
            "/export_objects.fcgi" => Results.File(Encoding.UTF8.GetBytes("object,id\nusers,1\ncards,1\n"), "application/zip", "stub-export.zip"),
            "/report_generate.fcgi" => Results.File(Encoding.UTF8.GetBytes("Name (User);Id (User)\nAda Lovelace;1\n"), "text/plain", "stub-report.txt"),
            "/export_afd.fcgi" => Results.File(Encoding.UTF8.GetBytes("0000000000000000000000000000000000000000000000000000000000000001\n"), "text/plain", "AFD1900001.txt"),
            "/export_audit_logs.fcgi" => Results.File(Encoding.UTF8.GetBytes("2026-04-13 10:00:00|config|Configuration changed\n"), "text/plain", "audit-logs.txt"),

            "/get_configuration.fcgi" => Results.Json(state.GetConfiguration(body)),
            "/set_configuration.fcgi" => Results.Json(state.SetConfiguration(body)),
            "/set_network_interlock.fcgi" => Success(),
            "/connection_test.fcgi" => Results.Json(new { success = true, host = "127.0.0.1", port = 80 }),
            "/ping_test.fcgi" => Results.Json(new { success = true, host = "127.0.0.1", latency_ms = 1 }),
            "/nslookup_test.fcgi" => Results.Json(new { success = true, host = "localhost", addresses = new[] { "127.0.0.1" } }),

            "/remote_user_authorization.fcgi" => Results.Json(new { success = true, authorized = true }),
            "/execute_actions.fcgi" => Results.Json(new { success = true, executed = true }),
            "/door_state.fcgi" => Results.Json(new { doors = new[] { new { door = 1, open = false, locked = false } } }),
            "/message_to_screen.fcgi" or "/buzzer_buzz.fcgi" or "/remote_led_control.fcgi" => Success(),
            "/remote_enroll.fcgi" => Results.Json(new { success = true, started = true }),
            "/cancel_remote_enroll.fcgi" => Results.Json(new { success = true, cancelled = true }),
            "/get_catra_info.fcgi" => Results.Json(new { clockwise = 10, anticlockwise = 8, entry = 6, exit = 5 }),
            "/upgrade_ten_thousand_face_templates.fcgi" => Results.Json(new { success = true, upgraded = "pro" }),
            "/idflex_upgrade_enterprise.fcgi" => Results.Json(new { success = true, upgraded = "enterprise" }),
            "/set_pjsip_audio_message.fcgi" or "/set_audio_access_message.fcgi" => Success(),
            "/get_pjsip_audio_message.fcgi" => Results.File(state.WavBytes, "audio/wav", "sip-ring.wav"),
            "/has_pjsip_audio_message.fcgi" => Results.Json(new { file_exists = true }),
            "/make_sip_call.fcgi" => Results.Json(state.MakeSipCall(body)),
            "/finalize_sip_call.fcgi" => Results.Json(state.FinalizeSipCall()),
            "/get_sip_status.fcgi" => Results.Json(state.GetSipStatus()),
            "/get_audio_access_message.fcgi" => Results.File(state.WavBytes, "audio/wav", "access-authorized.wav"),
            "/has_audio_access_messages.fcgi" => Results.Json(new { not_identified = true, authorized = true, not_authorized = true, use_mask = false }),

            "/user_get_image.fcgi" => Results.File(state.PngBytes, "image/png", "user_1.png"),
            "/user_list_images.fcgi" or "/user_list_images" => Results.Json(StubDatasetFactory.CreateImageInventory(runtime.DatasetSize, state.Timestamp)),
            "/user_get_image_list.fcgi" => Results.Json(new { user_images = new[] { new { id = 1L, timestamp = state.Timestamp, image = state.PngBase64 } } }),
            "/user_set_image.fcgi" => Results.Json(new { user_id = 1L, success = true, scores = state.PhotoScores }),
            "/user_set_image_list.fcgi" => Results.Json(new { results = new[] { new { user_id = 1L, success = true, scores = state.PhotoScores } } }),
            "/user_test_image.fcgi" => Results.Json(new { success = true, scores = state.PhotoScores }),
            "/user_destroy_image.fcgi" or "/logo_destroy.fcgi" => Results.Json(new { }),

            "/logo.fcgi" => Results.File(state.PngBytes, "image/png", "logo_1.png"),
            "/logo_change.fcgi" or "/send_video.fcgi" or "/set_custom_video.fcgi" or "/remove_custom_video.fcgi" => Success(),
            "/save_screenshot.fcgi" => Results.File(state.PngBytes, "image/png", "camera.png"),

            "/system_information.fcgi" => Results.Json(runtime.Profile.CreateSystemInformation(state.IdCloudCode)),
            "/change_login.fcgi" or "/set_system_time.fcgi" or "/reset_to_factory_default.fcgi" or
            "/reboot.fcgi" or "/reboot_recovery.fcgi" or "/delete_admins.fcgi" or
            "/set_system_network.fcgi" or "/ssl_certificate_change.fcgi" or
            "/set_vpn_information.fcgi" or "/set_vpn_file.fcgi" => Success(),
            "/get_vpn_information.fcgi" => Results.Json(new { enabled = true, login_enabled = true, login = "Admin" }),
            "/get_vpn_status.fcgi" => Results.Json(new { connected = true, status = "connected" }),
            "/has_vpn_file.fcgi" => Results.Json(new { has_vpn_file = true }),
            "/get_vpn_file.fcgi" => Results.File(Encoding.UTF8.GetBytes("client\nproto udp\nremote 127.0.0.1 1194\n"), "application/octet-stream", "client.conf"),
            "/change_idcloud_code.fcgi" => state.ChangeIdCloudCode(),

            "/gpio_state.fcgi" => Results.Json(new { gpio = 1, value = 0 }),
            "/reread_leds_settings.fcgi" => Success(),
            "/is_valid_biometry.fcgi" => Results.Json(new { success = true, score = 1000 }),
            "/alarm_status.fcgi" => Results.Json(state.GetAlarmStatus(body)),

            _ => Results.Json(new { success = true, path, method = request.Method })
        };
    }

    private static IResult Success() => Results.Json(new { success = true });
}
