using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:6600");

var app = builder.Build();
var state = new StubState();

app.MapMethods("/{**path}", ["GET", "POST"], async (HttpContext context) =>
{
    var request = context.Request;
    var path = request.Path.Value?.ToLowerInvariant() ?? "/";
    var bodyBytes = await ReadBodyAsync(request);
    var bodyJson = TryParseJson(bodyBytes);

    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.Method} {path}{request.QueryString}");

    return path switch
    {
        "/login.fcgi" => Results.Json(new { session = state.Session }),
        "/logout.fcgi" => Results.Json(new { }),
        "/session_is_valid.fcgi" => Results.Json(new { session_is_valid = true }),
        "/user_hash_password.fcgi" => Results.Json(new { password = "stub-hash", salt = "stub-salt" }),

        "/load_objects.fcgi" => Results.Json(state.LoadObjects(bodyJson)),
        "/create_objects.fcgi" => Results.Json(state.CreateObjects(bodyJson)),
        "/create_or_modify_objects.fcgi" => Results.Json(state.CreateOrModifyObjects(bodyJson)),
        "/modify_objects.fcgi" => Results.Json(state.ModifyObjects(bodyJson)),
        "/destroy_objects.fcgi" => Results.Json(state.DestroyObjects(bodyJson)),
        "/export_objects.fcgi" => Results.File(
            Encoding.UTF8.GetBytes("object,id\nusers,1\ncards,1\n"),
            "application/zip",
            "stub-export.zip"),
        "/report_generate.fcgi" => Results.File(
            Encoding.UTF8.GetBytes("Name (User);Id (User)\nAda Lovelace;1\n"),
            "text/plain",
            "stub-report.txt"),
        "/export_afd.fcgi" => Results.File(
            Encoding.UTF8.GetBytes("0000000000000000000000000000000000000000000000000000000000000001\n"),
            "text/plain",
            "AFD1900001.txt"),
        "/export_audit_logs.fcgi" => Results.File(
            Encoding.UTF8.GetBytes("2026-04-13 10:00:00|config|Configuration changed\n"),
            "text/plain",
            "audit-logs.txt"),

        "/get_configuration.fcgi" => Results.Json(state.GetConfiguration(bodyJson)),
        "/set_configuration.fcgi" => Results.Json(state.SetConfiguration(bodyJson)),
        "/set_network_interlock.fcgi" => Results.Json(new { success = true }),
        "/connection_test.fcgi" => Results.Json(new { success = true, host = "127.0.0.1", port = 80 }),
        "/ping_test.fcgi" => Results.Json(new { success = true, host = "127.0.0.1", latency_ms = 1 }),
        "/nslookup_test.fcgi" => Results.Json(new { success = true, host = "localhost", addresses = new[] { "127.0.0.1" } }),

        "/remote_user_authorization.fcgi" => Results.Json(new { success = true, authorized = true }),
        "/execute_actions.fcgi" => Results.Json(new { success = true, executed = true }),
        "/door_state.fcgi" => Results.Json(new { doors = new[] { new { door = 1, open = false, locked = false } } }),
        "/message_to_screen.fcgi" => Results.Json(new { success = true }),
        "/buzzer_buzz.fcgi" => Results.Json(new { success = true }),
        "/remote_enroll.fcgi" => Results.Json(new { success = true, started = true }),
        "/cancel_remote_enroll.fcgi" => Results.Json(new { success = true, cancelled = true }),
        "/get_catra_info.fcgi" => Results.Json(new { clockwise = 10, anticlockwise = 8, entry = 6, exit = 5 }),
        "/remote_led_control.fcgi" => Results.Json(new { success = true }),
        "/upgrade_ten_thousand_face_templates.fcgi" => Results.Json(new { success = true, upgraded = "pro" }),
        "/idflex_upgrade_enterprise.fcgi" => Results.Json(new { success = true, upgraded = "enterprise" }),
        "/set_pjsip_audio_message.fcgi" => Results.Json(new { success = true }),
        "/get_pjsip_audio_message.fcgi" => Results.File(state.WavBytes, "audio/wav", "sip-ring.wav"),
        "/has_pjsip_audio_message.fcgi" => Results.Json(new { file_exists = true }),
        "/make_sip_call.fcgi" => Results.Json(state.MakeSipCall(bodyJson)),
        "/finalize_sip_call.fcgi" => Results.Json(state.FinalizeSipCall()),
        "/get_sip_status.fcgi" => Results.Json(state.GetSipStatus()),
        "/set_audio_access_message.fcgi" => Results.Json(new { success = true }),
        "/get_audio_access_message.fcgi" => Results.File(state.WavBytes, "audio/wav", "access-authorized.wav"),
        "/has_audio_access_messages.fcgi" => Results.Json(new { not_identified = true, authorized = true, not_authorized = true, use_mask = false }),

        "/user_get_image.fcgi" => Results.File(state.PngBytes, "image/png", "user_1.png"),
        "/user_list_images.fcgi" or "/user_list_images" => Results.Json(new { user_ids = new[] { 1L }, image_info = new[] { new { user_id = 1L, timestamp = state.Timestamp } } }),
        "/user_get_image_list.fcgi" => Results.Json(new { user_images = new[] { new { id = 1L, timestamp = state.Timestamp, image = state.PngBase64 } } }),
        "/user_set_image.fcgi" => Results.Json(new { user_id = 1L, success = true, scores = state.PhotoScores }),
        "/user_set_image_list.fcgi" => Results.Json(new { results = new[] { new { user_id = 1L, success = true, scores = state.PhotoScores } } }),
        "/user_test_image.fcgi" => Results.Json(new { success = true, scores = state.PhotoScores }),
        "/user_destroy_image.fcgi" => Results.Json(new { }),

        "/logo.fcgi" => Results.File(state.PngBytes, "image/png", "logo_1.png"),
        "/logo_change.fcgi" => Results.Json(new { success = true }),
        "/logo_destroy.fcgi" => Results.Json(new { }),
        "/send_video.fcgi" => Results.Json(new { success = true }),
        "/set_custom_video.fcgi" => Results.Json(new { success = true }),
        "/remove_custom_video.fcgi" => Results.Json(new { success = true }),
        "/save_screenshot.fcgi" => Results.File(state.PngBytes, "image/png", "camera.png"),

        "/system_information.fcgi" => Results.Json(new
        {
            serial = "1900001",
            version = "6.2.0-stub",
            product_name = "iDFace Stub",
            iDCloud_code = state.IdCloudCode,
            device_id = "stub-device",
            online = true,
            network = new
            {
                ip = "127.0.0.1",
                gateway = "127.0.0.1",
                netmask = "255.255.255.0",
                mac = "00:11:22:33:44:55"
            }
        }),
        "/change_login.fcgi" => Results.Json(new { success = true }),
        "/set_system_time.fcgi" => Results.Json(new { success = true }),
        "/reset_to_factory_default.fcgi" => Results.Json(new { success = true }),
        "/reboot.fcgi" => Results.Json(new { success = true }),
        "/reboot_recovery.fcgi" => Results.Json(new { success = true }),
        "/delete_admins.fcgi" => Results.Json(new { success = true }),
        "/set_system_network.fcgi" => Results.Json(new { success = true }),
        "/ssl_certificate_change.fcgi" => Results.Json(new { success = true }),
        "/set_vpn_information.fcgi" => Results.Json(new { success = true }),
        "/get_vpn_information.fcgi" => Results.Json(new { enabled = true, login_enabled = true, login = "Admin" }),
        "/get_vpn_status.fcgi" => Results.Json(new { connected = true, status = "connected" }),
        "/has_vpn_file.fcgi" => Results.Json(new { has_vpn_file = true }),
        "/set_vpn_file.fcgi" => Results.Json(new { success = true }),
        "/get_vpn_file.fcgi" => Results.File(Encoding.UTF8.GetBytes("client\nproto udp\nremote 127.0.0.1 1194\n"), "application/octet-stream", "client.conf"),
        "/change_idcloud_code.fcgi" => state.ChangeIdCloudCode(),

        "/gpio_state.fcgi" => Results.Json(new { gpio = 1, value = 0 }),
        "/reread_leds_settings.fcgi" => Results.Json(new { success = true }),
        "/is_valid_biometry.fcgi" => Results.Json(new { success = true, score = 1000 }),
        "/alarm_status.fcgi" => Results.Json(state.GetAlarmStatus(bodyJson)),

        _ => Results.Json(new { success = true, path, method = request.Method })
    };
});

app.Run();

static async Task<byte[]> ReadBodyAsync(HttpRequest request)
{
    if (request.ContentLength is 0)
        return [];

    using var memory = new MemoryStream();
    await request.Body.CopyToAsync(memory);
    return memory.ToArray();
}

static JsonNode? TryParseJson(byte[] bodyBytes)
{
    if (bodyBytes.Length == 0)
        return null;

    try
    {
        return JsonNode.Parse(bodyBytes);
    }
    catch
    {
        return null;
    }
}

sealed class StubState
{
    public string Session { get; } = "stub-session";
    public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public byte[] PngBytes { get; } = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR8kAAAAASUVORK5CYII=");
    public byte[] WavBytes { get; } = Encoding.ASCII.GetBytes("RIFFSTUBWAVEfmt data");
    public string PngBase64 => Convert.ToBase64String(PngBytes);
    public object PhotoScores { get; } = new { bounds_width = 140, horizontal_center_offset = 0, vertical_center_offset = 0, center_pose_quality = 900, sharpness_quality = 950 };
    public string IdCloudCode { get; private set; } = "CID-STUB-0001";

    private readonly Dictionary<string, JsonArray> _objects;
    private readonly JsonObject _configuration;
    private bool _alarmActive = true;
    private int _alarmCause = 7;
    private bool _sipInCall;
    private int _sipStatus = 200;

    public StubState()
    {
        _objects = new Dictionary<string, JsonArray>(StringComparer.OrdinalIgnoreCase)
        {
            ["users"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["registration"] = "1001",
                    ["name"] = "Ada Lovelace",
                    ["email"] = "ada@example.com",
                    ["phone"] = "5511999999999",
                    ["status"] = "active",
                    ["user_type_id"] = 1,
                    ["begin_time"] = 1704067200,
                    ["end_time"] = 1893456000,
                    ["created_at"] = "2026-04-13T10:00:00Z",
                    ["updated_at"] = "2026-04-13T10:00:00Z"
                }
            ],
            ["groups"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Administrators",
                    ["created_at"] = "2026-04-13T10:00:00Z",
                    ["updated_at"] = "2026-04-13T10:00:00Z"
                }
            ],
            ["cards"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["value"] = "1234567890",
                    ["type"] = 1,
                    ["status"] = "active",
                    ["created_at"] = "2026-04-13T10:00:00Z",
                    ["updated_at"] = "2026-04-13T10:00:00Z"
                }
            ],
            ["templates"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["finger_position"] = 1,
                    ["template"] = "ZmFrZS10ZW1wbGF0ZQ==",
                    ["created_at"] = "2026-04-13T10:00:00Z"
                }
            ],
            ["qrcodes"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["value"] = "QR-CODE-1",
                    ["begin_time"] = 1704067200,
                    ["end_time"] = 1893456000
                }
            ],
            ["uhf_tags"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["value"] = "CAFEDAD0"
                }
            ],
            ["pins"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["value"] = "123456"
                }
            ],
            ["alarm_zones"] =
            [
                new JsonObject
                {
                    ["zone"] = 1,
                    ["enabled"] = 1,
                    ["active_level"] = 0,
                    ["alarm_delay"] = 5
                }
            ],
            ["user_roles"] =
            [
                new JsonObject
                {
                    ["user_id"] = 1,
                    ["role"] = 1
                }
            ],
            ["user_groups"] =
            [
                new JsonObject
                {
                    ["user_id"] = 1,
                    ["group_id"] = 1
                }
            ],
            ["scheduled_unlocks"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Morning Open",
                    ["message"] = "Open lobby"
                }
            ],
            ["actions"] =
            [
                new JsonObject
                {
                    ["group_id"] = 1,
                    ["name"] = "Open Door",
                    ["action"] = "door",
                    ["parameters"] = "door=1",
                    ["run_at"] = 0
                }
            ],
            ["areas"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Lobby"
                }
            ],
            ["portals"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Portal A",
                    ["area_from_id"] = 1,
                    ["area_to_id"] = 2
                }
            ],
            ["portal_actions"] =
            [
                new JsonObject
                {
                    ["portal_id"] = 1,
                    ["action_id"] = 1
                }
            ],
            ["access_rules"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Office Hours",
                    ["priority"] = 1,
                    ["type"] = 0,
                    ["created_at"] = "2026-04-13T10:00:00Z"
                }
            ],
            ["portal_access_rules"] =
            [
                new JsonObject
                {
                    ["portal_id"] = 1,
                    ["access_rule_id"] = 1
                }
            ],
            ["group_access_rules"] =
            [
                new JsonObject
                {
                    ["group_id"] = 1,
                    ["access_rule_id"] = 1
                }
            ],
            ["scheduled_unlock_access_rules"] =
            [
                new JsonObject
                {
                    ["scheduled_unlock_id"] = 1,
                    ["access_rule_id"] = 1
                }
            ],
            ["time_zones"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Business Hours"
                }
            ],
            ["time_spans"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["time_zone_id"] = 1,
                    ["start"] = 28800,
                    ["end"] = 64800,
                    ["sun"] = 0,
                    ["mon"] = 1,
                    ["tue"] = 1,
                    ["wed"] = 1,
                    ["thu"] = 1,
                    ["fri"] = 1,
                    ["sat"] = 0,
                    ["hol1"] = 0,
                    ["hol2"] = 0,
                    ["hol3"] = 0
                }
            ],
            ["contingency_cards"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["value"] = 819876543210
                }
            ],
            ["contingency_card_access_rules"] =
            [
                new JsonObject
                {
                    ["access_rule_id"] = 1
                }
            ],
            ["holidays"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Holiday",
                    ["start"] = 1735689600,
                    ["end"] = 1735776000,
                    ["hol1"] = 1,
                    ["hol2"] = 0,
                    ["hol3"] = 0,
                    ["repeats"] = 1
                }
            ],
            ["alarm_zone_time_zones"] =
            [
                new JsonObject
                {
                    ["alarm_zone_id"] = 1,
                    ["time_zone_id"] = 1
                }
            ],
            ["access_rule_time_zones"] =
            [
                new JsonObject
                {
                    ["access_rule_id"] = 1,
                    ["time_zone_id"] = 1
                }
            ],
            ["devices"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Front Door",
                    ["ip"] = "127.0.0.1",
                    ["port"] = 6600,
                    ["enabled"] = 1
                }
            ],
            ["user_access_rules"] =
            [
                new JsonObject
                {
                    ["user_id"] = 1,
                    ["access_rule_id"] = 1
                }
            ],
            ["area_access_rules"] =
            [
                new JsonObject
                {
                    ["area_id"] = 1,
                    ["access_rule_id"] = 1
                }
            ],
            ["catra_infos"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["left_turns"] = 10,
                    ["right_turns"] = 8,
                    ["entrance_turns"] = 6,
                    ["exit_turns"] = 5
                }
            ],
            ["log_types"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Check In"
                }
            ],
            ["sec_boxs"] =
            [
                new JsonObject
                {
                    ["id"] = 65793,
                    ["version"] = 2,
                    ["name"] = "SecBox",
                    ["enabled"] = true,
                    ["relay_timeout"] = 3000,
                    ["door_sensor_enabled"] = true,
                    ["door_sensor_idle"] = false,
                    ["auto_close_enabled"] = 1
                }
            ],
            ["contacts"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Reception",
                    ["number"] = "500"
                }
            ],
            ["timed_alarms"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Alarm Morning",
                    ["start"] = 28800,
                    ["sun"] = 0,
                    ["mon"] = 1,
                    ["tue"] = 1,
                    ["wed"] = 1,
                    ["thu"] = 1,
                    ["fri"] = 1,
                    ["sat"] = 0
                }
            ],
            ["access_events"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["event"] = "door",
                    ["type"] = "OPEN",
                    ["identification"] = "1",
                    ["device_id"] = 1,
                    ["timestamp"] = 1713000000
                }
            ],
            ["custom_thresholds"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["threshold"] = 1200
                }
            ],
            ["network_interlocking_rules"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["ip"] = "192.168.0.20",
                    ["login"] = "admin",
                    ["password"] = "admin",
                    ["portal_name"] = "Airlock B",
                    ["enabled"] = 1
                }
            ],
            ["config_groups"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["group"] = "general",
                    ["key"] = "show_logo",
                    ["value"] = "1"
                }
            ],
            ["logs"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["event"] = 7,
                    ["event_name"] = "authorized",
                    ["time"] = 1713000000
                }
            ],
            ["access_log_access_rules"] =
            [
                new JsonObject
                {
                    ["access_log_id"] = 1,
                    ["access_rule_id"] = 1
                }
            ],
            ["alarm_logs"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["event"] = 1,
                    ["cause"] = 7,
                    ["user_id"] = 1,
                    ["time"] = 1713000000,
                    ["door_id"] = 1
                }
            ],
            ["change_logs"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["user_id"] = 1,
                    ["object"] = "users",
                    ["operation"] = "modify",
                    ["time"] = 1713000100
                }
            ],
            ["catra_events"] =
            [
                new JsonObject
                {
                    ["id"] = 1,
                    ["event"] = "clockwise",
                    ["time"] = 1713000200,
                    ["user_id"] = 1
                }
            ]
        };

        _configuration = new JsonObject
        {
            ["general"] = new JsonObject
            {
                ["show_logo"] = "1",
                ["keep_user_image"] = "1",
                ["ssh_enabled"] = "1",
                ["beep_enabled"] = "1",
                ["attendance_mode"] = "0",
                ["clear_expired_users"] = "visitors",
                ["online"] = "1",
                ["local_identification"] = "1",
                ["usb_port_enabled"] = "1",
                ["web_server_enabled"] = "1",
                ["screen_always_on"] = "1",
                ["sec_box_out_mode"] = "0",
                ["relay_out_mode"] = "0",
                ["relay1_enabled"] = "1",
                ["relay1_auto_close"] = "1",
                ["relay1_timeout"] = "3000",
                ["screenshot_resize"] = "0.42",
                ["energy_mode"] = "0",
                ["energy_display_custom"] = "6",
                ["energy_sound_custom"] = "6",
                ["energy_ir_custom"] = "10",
                ["energy_led_custom"] = "10",
                ["gpio_ext1_mode"] = "7",
                ["gpio_ext1_idle"] = "0",
                ["gpio_ext2_mode"] = "0",
                ["gpio_ext2_idle"] = "0",
                ["gpio_ext3_mode"] = "8",
                ["gpio_ext3_idle"] = "1"
            },
            ["ntp"] = new JsonObject
            {
                ["enabled"] = "1",
                ["server"] = "pool.ntp.org"
            },
            ["monitor"] = new JsonObject
            {
                ["hostname"] = "127.0.0.1",
                ["port"] = "5001",
                ["path"] = "api/notifications"
            },
            ["video_player"] = new JsonObject
            {
                ["custom_video_enabled"] = "1"
            },
            ["sec_box"] = new JsonObject
            {
                ["catra_role"] = "1",
                ["catra_default_fsm"] = "0",
                ["catra_timeout"] = "1000",
                ["catra_collect_visitor_card"] = "0"
            },
            ["identifier"] = new JsonObject
            {
                ["custom_auth_message"] = "Welcome",
                ["enable_custom_auth_message"] = "1",
                ["log_type"] = "0",
                ["card_identification_enabled"] = "1",
                ["face_identification_enabled"] = "1",
                ["qrcode_identification_enabled"] = "1",
                ["pin_identification_enabled"] = "0"
            },
            ["online_client"] = new JsonObject
            {
                ["server_id"] = "1",
                ["extract_template"] = "0",
                ["max_request_attempts"] = "3"
            },
            ["push_server"] = new JsonObject
            {
                ["push_remote_address"] = "https://push.idsecure.com.br/api",
                ["push_request_timeout"] = "30000",
                ["push_request_period"] = "5"
            },
            ["snmp_agent"] = new JsonObject
            {
                ["snmp_enabled"] = "0"
            },
            ["alarm"] = new JsonObject
            {
                ["device_violation_enabled"] = "0",
                ["door_sensor_alarm_timeout_after_closure"] = "0",
                ["door_sensor_delay"] = "5",
                ["door_sensor_enabled"] = "1",
                ["forced_access_debounce"] = "2",
                ["forced_access_enabled"] = "0",
                ["panic_card_enabled"] = "1",
                ["panic_finger_delay"] = "1",
                ["panic_finger_enabled"] = "1",
                ["panic_password_enabled"] = "1",
                ["panic_pin_enabled"] = "1"
            },
            ["face_id"] = new JsonObject
            {
                ["mask_detection_enabled"] = "0",
                ["vehicle_mode"] = "0",
                ["max_identified_duration"] = "30000",
                ["limit_identification_to_display_region"] = "0",
                ["min_detect_bounds_width"] = "0.29",
                ["qrcode_legacy_mode_enabled"] = "0",
                ["totp_enabled"] = "0",
                ["totp_window_size"] = "30",
                ["totp_window_num"] = "5",
                ["totp_single_use"] = "1",
                ["totp_tz_offset"] = "0"
            },
            ["barras"] = new JsonObject
            {
                ["qrcode_legacy_mode_enabled"] = "1",
                ["totp_enabled"] = "0",
                ["totp_window_size"] = "30",
                ["totp_window_num"] = "5",
                ["totp_single_use"] = "1",
                ["totp_tz_offset"] = "0"
            },
            ["camera_overlay"] = new JsonObject
            {
                ["zoom"] = "1.00",
                ["vertical_crop"] = "0.00"
            },
            ["face_module"] = new JsonObject
            {
                ["light_threshold_led_activation"] = "40"
            },
            ["led_white"] = new JsonObject
            {
                ["brightness"] = "70"
            },
            ["onvif"] = new JsonObject
            {
                ["rtsp_enabled"] = "0",
                ["rtsp_port"] = "554",
                ["rtsp_username"] = "admin",
                ["rtsp_password"] = "admin",
                ["rtsp_rgb"] = "1",
                ["rtsp_codec"] = "h264",
                ["onvif_enabled"] = "0",
                ["onvif_port"] = "8000",
                ["rtsp_flipped"] = "0",
                ["rtsp_watermark_enabled"] = "1",
                ["rtsp_watermark_logo_enabled"] = "1",
                ["rtsp_watermark_custom_logo_enabled"] = "0"
            },
            ["pjsip"] = new JsonObject
            {
                ["enabled"] = "0",
                ["server_ip"] = "sip.example.com",
                ["server_port"] = "5060",
                ["server_outbound_port"] = "10000",
                ["server_outbound_port_range"] = "1000",
                ["numeric_branch_enabled"] = "1",
                ["branch"] = "987",
                ["login"] = "987",
                ["password"] = "123456",
                ["peer_to_peer_enabled"] = "0",
                ["reg_status_query_period"] = "60",
                ["server_retry_interval"] = "5",
                ["max_call_time"] = "300",
                ["push_button_debounce"] = "50",
                ["auto_answer_enabled"] = "0",
                ["auto_answer_delay"] = "5",
                ["auto_call_button_enabled"] = "1",
                ["rex_enabled"] = "0",
                ["dialing_display_mode"] = "0",
                ["auto_call_target"] = "456",
                ["custom_identifier_auto_call"] = "Reception",
                ["video_enabled"] = "0",
                ["pjsip_custom_audio_enabled"] = "0",
                ["custom_audio_volume_gain"] = "1",
                ["mic_volume"] = "5",
                ["speaker_volume"] = "7",
                ["open_door_enabled"] = "0",
                ["open_door_command"] = "12345",
                ["facial_id_during_call_enabled"] = "0"
            },
            ["buzzer"] = new JsonObject
            {
                ["audio_message_not_identified"] = "default",
                ["audio_message_authorized"] = "default",
                ["audio_message_not_authorized"] = "default",
                ["audio_message_use_mask"] = "default",
                ["audio_message_volume_gain"] = "2"
            }
        };
    }

    public object LoadObjects(JsonNode? body)
    {
        var objectName = body?["object"]?.GetValue<string>() ?? string.Empty;
        var array = GetObjectArray(objectName);
        var where = body?["where"]?[objectName] as JsonObject;

        var filtered = new JsonArray();
        foreach (var item in array)
        {
            if (!MatchesWhere(item as JsonObject, where))
                continue;

            filtered.Add(item?.DeepClone());
        }

        return new Dictionary<string, object?>
        {
            [objectName] = JsonSerializer.Deserialize<object?>(filtered.ToJsonString()) ?? Array.Empty<object>()
        };
    }

    public object CreateObjects(JsonNode? body)
    {
        var objectName = body?["object"]?.GetValue<string>() ?? string.Empty;
        var values = body?["values"];
        var ids = new List<long>();
        var array = GetObjectArray(objectName);

        foreach (var node in EnumerateNodes(values))
        {
            var clone = (node?.DeepClone() as JsonObject) ?? new JsonObject();
            var id = TryGetInt64(clone["id"]) ?? GetNextId(array);
            clone["id"] = id;
            ids.Add(id);
            array.Add(clone);
        }

        return new { ids };
    }

    public object CreateOrModifyObjects(JsonNode? body)
    {
        var objectName = body?["object"]?.GetValue<string>() ?? string.Empty;
        var values = body?["values"];
        var array = GetObjectArray(objectName);
        var changes = 0;

        foreach (var node in EnumerateNodes(values))
        {
            var incoming = (node?.DeepClone() as JsonObject) ?? new JsonObject();
            var existing = FindExistingObject(array, incoming, objectName);

            if (existing != null)
            {
                Merge(existing, incoming);
                changes++;
                continue;
            }

            var id = TryGetInt64(incoming["id"]);
            if (!id.HasValue && UsesNumericIdentity(objectName))
                incoming["id"] = GetNextId(array);

            array.Add(incoming);
            changes++;
        }

        return new { changes };
    }

    public object ModifyObjects(JsonNode? body)
    {
        var objectName = body?["object"]?.GetValue<string>() ?? string.Empty;
        var where = body?["where"]?[objectName] as JsonObject;
        var values = body?["values"] as JsonObject;
        var array = GetObjectArray(objectName);

        if (where != null && values != null)
        {
            foreach (var item in array.OfType<JsonObject>())
            {
                if (!MatchesWhere(item, where))
                    continue;

                Merge(item, values);
                break;
            }
        }

        return new { success = true };
    }

    public object DestroyObjects(JsonNode? body)
    {
        var objectName = body?["object"]?.GetValue<string>() ?? string.Empty;
        var where = body?["where"]?[objectName] as JsonObject;
        var array = GetObjectArray(objectName);

        if (where != null)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (MatchesWhere(array[index] as JsonObject, where))
                    array.RemoveAt(index);
            }
        }

        return new { success = true };
    }

    public object GetConfiguration(JsonNode? body)
    {
        if (body is not JsonObject requested || requested.Count == 0)
            return DeserializeObject(_configuration);

        var result = new JsonObject();
        foreach (var property in requested)
        {
            if (_configuration[property.Key] != null)
                result[property.Key] = _configuration[property.Key]?.DeepClone();
        }

        return DeserializeObject(result);
    }

    public object SetConfiguration(JsonNode? body)
    {
        if (body is JsonObject updates)
        {
            foreach (var property in updates)
            {
                if (_configuration[property.Key] is JsonObject existing && property.Value is JsonObject incoming)
                {
                    Merge(existing, incoming);
                    continue;
                }

                _configuration[property.Key] = property.Value?.DeepClone();
            }
        }

        return DeserializeObject(_configuration);
    }

    public IResult ChangeIdCloudCode()
    {
        IdCloudCode = $"CID-STUB-{Random.Shared.Next(1000, 9999)}";
        return Results.Json(new { success = true });
    }

    public object GetAlarmStatus(JsonNode? body)
    {
        var stop = body?["stop"]?.GetValue<bool?>() ?? false;
        if (stop)
        {
            _alarmActive = false;
            _alarmCause = 0;
        }

        return new
        {
            active = _alarmActive,
            cause = _alarmCause
        };
    }

    public object MakeSipCall(JsonNode? body)
    {
        _sipInCall = true;
        _sipStatus = 200;
        return new
        {
            success = true,
            target = body?["target"]?.GetValue<string>() ?? string.Empty
        };
    }

    public object FinalizeSipCall()
    {
        _sipInCall = false;
        _sipStatus = 200;
        return new { success = true };
    }

    public object GetSipStatus()
    {
        return new
        {
            status = _sipStatus,
            in_call = _sipInCall
        };
    }

    private static object DeserializeObject(JsonNode node)
    {
        return JsonSerializer.Deserialize<object>(node.ToJsonString()) ?? new { };
    }

    private JsonArray GetObjectArray(string objectName)
    {
        if (!_objects.TryGetValue(objectName, out var array))
        {
            array = new JsonArray();
            _objects[objectName] = array;
        }

        return array;
    }

    private static IEnumerable<JsonNode?> EnumerateNodes(JsonNode? node)
    {
        return node switch
        {
            JsonArray array => array,
            JsonObject obj => [obj],
            _ => []
        };
    }

    private static long? TryGetInt64(JsonNode? node)
    {
        if (node == null)
            return null;

        return node switch
        {
            JsonValue value when value.TryGetValue<long>(out var int64Value) => int64Value,
            JsonValue value when value.TryGetValue<int>(out var int32Value) => int32Value,
            JsonValue value when value.TryGetValue<string>(out var stringValue) && long.TryParse(stringValue, out var parsed) => parsed,
            _ => null
        };
    }

    private static long GetNextId(JsonArray array)
    {
        var maxId = array
            .Select(item => TryGetInt64(item?["id"]))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .DefaultIfEmpty(0)
            .Max();

        return maxId + 1;
    }

    private static JsonObject? FindExistingObject(JsonArray array, JsonObject incoming, string objectName)
    {
        foreach (var item in array.OfType<JsonObject>())
        {
            var matchKeys = GetMatchKeys(objectName);
            if (matchKeys.All(key => string.Equals(item[key]?.ToJsonString() ?? string.Empty, incoming[key]?.ToJsonString() ?? string.Empty, StringComparison.Ordinal)))
                return item;
        }

        return null;
    }

    private static bool MatchesWhere(JsonObject? item, JsonObject? where)
    {
        if (item == null)
            return false;

        if (where == null || where.Count == 0)
            return true;

        foreach (var property in where)
        {
            var existingValue = item[property.Key]?.ToJsonString() ?? string.Empty;
            var requestedValue = property.Value?.ToJsonString() ?? string.Empty;
            if (!string.Equals(existingValue, requestedValue, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static bool UsesNumericIdentity(string objectName)
    {
        return GetMatchKeys(objectName).Contains("id", StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> GetMatchKeys(string objectName)
    {
        return objectName.ToLowerInvariant() switch
        {
            "user_roles" => ["user_id"],
            "user_groups" => ["user_id", "group_id"],
            "actions" => ["group_id"],
            "portal_actions" => ["portal_id", "action_id"],
            "portal_access_rules" => ["portal_id", "access_rule_id"],
            "group_access_rules" => ["group_id", "access_rule_id"],
            "scheduled_unlock_access_rules" => ["scheduled_unlock_id", "access_rule_id"],
            "alarm_zones" => ["zone"],
            "alarm_zone_time_zones" => ["alarm_zone_id", "time_zone_id"],
            "access_rule_time_zones" => ["access_rule_id", "time_zone_id"],
            "contingency_card_access_rules" => ["access_rule_id"],
            "user_access_rules" => ["user_id", "access_rule_id"],
            "area_access_rules" => ["area_id", "access_rule_id"],
            "sec_boxs" => ["id"],
            "custom_thresholds" => ["user_id"],
            "network_interlocking_rules" => ["id"],
            "pins" => ["id"],
            "uhf_tags" => ["id"],
            _ => ["id"]
        };
    }

    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var property in source)
            target[property.Key] = property.Value?.DeepClone();
    }
}
