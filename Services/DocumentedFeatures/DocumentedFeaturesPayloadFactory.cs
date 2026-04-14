using Integracao.ControlID.PoC.ViewModels.DocumentedFeatures;

namespace Integracao.ControlID.PoC.Services.DocumentedFeatures
{
    public sealed class DocumentedFeaturesPayloadFactory
    {
        public object BuildAttendanceSettings(DocumentedFeaturesViewModel model)
        {
            return new
            {
                general = new
                {
                    attendance_mode = BoolString(model.AttendanceModeEnabled),
                    clear_expired_users = model.AttendanceClearExpiredUsers
                },
                identifier = new
                {
                    log_type = BoolString(model.AttendanceCustomLogTypesEnabled)
                }
            };
        }

        public object BuildOnlineSettings(DocumentedFeaturesViewModel model, long serverId)
        {
            return new
            {
                general = new
                {
                    online = BoolString(model.OnlineEnabled),
                    local_identification = BoolString(model.OnlineLocalIdentification)
                },
                online_client = new
                {
                    server_id = serverId.ToString(),
                    extract_template = BoolString(model.OnlineExtractTemplate),
                    max_request_attempts = model.OnlineMaxRequestAttempts.ToString()
                }
            };
        }

        public object BuildSecuritySettings(DocumentedFeaturesViewModel model)
        {
            return new
            {
                general = new
                {
                    ssh_enabled = BoolString(model.SecuritySshEnabled),
                    usb_port_enabled = BoolString(model.SecurityUsbPortEnabled),
                    web_server_enabled = BoolString(model.SecurityWebServerEnabled)
                },
                snmp_agent = new
                {
                    snmp_enabled = BoolString(model.SecuritySnmpEnabled)
                }
            };
        }

        public object BuildVisitorsSettings(DocumentedFeaturesViewModel model)
        {
            return new
            {
                general = new
                {
                    clear_expired_users = model.VisitorsClearExpiredUsers
                },
                sec_box = new
                {
                    catra_collect_visitor_card = BoolString(model.VisitorsCollectCardOnExit)
                }
            };
        }

        public object BuildIdCloudSettings(DocumentedFeaturesViewModel model)
        {
            return new
            {
                push_server = new
                {
                    push_remote_address = model.IdCloudPushRemoteAddress,
                    push_request_timeout = model.IdCloudPushRequestTimeout.ToString(),
                    push_request_period = model.IdCloudPushRequestPeriod.ToString()
                }
            };
        }

        public object BuildAlarmSettings(DocumentedFeaturesViewModel model)
        {
            return new
            {
                alarm = new
                {
                    device_violation_enabled = BoolString(model.AlarmDeviceViolationEnabled),
                    door_sensor_alarm_timeout_after_closure = model.AlarmDoorSensorAlarmTimeoutAfterClosure.ToString(),
                    door_sensor_delay = model.AlarmDoorSensorDelay.ToString(),
                    door_sensor_enabled = BoolString(model.AlarmDoorSensorEnabled),
                    forced_access_debounce = model.AlarmForcedAccessDebounce.ToString(),
                    forced_access_enabled = BoolString(model.AlarmForcedAccessEnabled),
                    panic_card_enabled = BoolString(model.AlarmPanicCardEnabled),
                    panic_finger_delay = model.AlarmPanicFingerDelay.ToString(),
                    panic_finger_enabled = BoolString(model.AlarmPanicFingerEnabled),
                    panic_password_enabled = BoolString(model.AlarmPanicPasswordEnabled),
                    panic_pin_enabled = BoolString(model.AlarmPanicPinEnabled)
                }
            };
        }

        public object BuildAfdExport(DocumentedFeaturesViewModel model)
        {
            var payload = new Dictionary<string, object>
            {
                ["mode"] = model.AfdMode
            };

            if (model.AfdInitialNsr.HasValue)
            {
                payload["initial_nsr"] = model.AfdInitialNsr.Value;
            }

            if (model.AfdInitialDate.HasValue)
            {
                payload["initial_date"] = new
                {
                    day = model.AfdInitialDate.Value.Day,
                    month = model.AfdInitialDate.Value.Month,
                    year = model.AfdInitialDate.Value.Year
                };
            }

            return payload;
        }

        public object BuildAuditLogsExport(DocumentedFeaturesViewModel model)
        {
            return new
            {
                config = model.AuditConfig ? 1 : 0,
                api = model.AuditApi ? 1 : 0,
                usb = model.AuditUsb ? 1 : 0,
                network = model.AuditNetwork ? 1 : 0,
                time = model.AuditTime ? 1 : 0,
                online = model.AuditOnline ? 1 : 0,
                menu = model.AuditMenu ? 1 : 0
            };
        }

        private static string BoolString(bool value) => value ? "1" : "0";
    }
}
