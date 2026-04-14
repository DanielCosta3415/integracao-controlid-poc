# Auditoria de cobertura da API Control iD

Data da auditoria: 2026-04-13

## Referências oficiais

- Índice atual da documentação da API de acesso: https://www.controlid.com.br/docs/access-api-en/
- Import/Export data (`export_objects.fcgi`): https://www.controlid.com.br/docs/access-api-en/system/synchronization/
- Camera image capture (`save_screenshot.fcgi`): https://www.controlid.com.br/docs/access-api-en/facial-recognition/camera-capture/
- Facial photo enrollment (`user_get_image_list.fcgi`, `user_set_image_list.fcgi`, `user_test_image.fcgi`): https://www.controlid.com.br/docs/access-api-en/facial-recognition/facial-enroll/
- Network Interlocking (`set_network_interlock.fcgi`): https://www.controlid.com.br/docs/access-api-en/configurations/network-interlocking/
- Security Hardening (`set_configuration.fcgi` com `ssh_enabled`): https://www.controlid.com.br/docs/access-api-en/system/security-hardening/
- Customize Event Messages (`set_configuration.fcgi` com `identifier.*`): https://www.controlid.com.br/docs/access-api-en/actions/customize-event-messages/
- Create or Modify Objects (`create_or_modify_objects.fcgi`): https://www.controlid.com.br/docs/access-api-en/objects/create-or-modify-objects/
- List of Objects: https://www.controlid.com.br/docs/access-api-en/objects/list-of-objects/
- QR Code and TOTP (`qrcode_legacy_mode_enabled`, `totp_*`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/qr-code/
- Upgrade iDFace (`upgrade_ten_thousand_face_templates.fcgi`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idface/
- Upgrade iDFlex/iDAccess Nano (`idflex_upgrade_enterprise.fcgi`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idflex-idaccess-nano/
- Streaming iDFace (`onvif.*` via `set_configuration.fcgi`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/streaming-idface/
- SIP intercom iDFace (`set_pjsip_audio_message.fcgi`, `get_pjsip_audio_message.fcgi`, `has_pjsip_audio_message.fcgi`, `make_sip_call.fcgi`, `finalize_sip_call.fcgi`, `get_sip_status.fcgi`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/sip-intercom-idface/
- iDFace access sound messages (`set_audio_access_message.fcgi`, `get_audio_access_message.fcgi`, `has_audio_access_messages.fcgi`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/access-audio-messages-idface/
- Custom signals iDFace Max (`general.sec_box_out_mode`, `general.relay_out_mode`, `general.gpio_ext*`): https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/custom-signals-idface-max/
- Power iDFace Max (`general.energy_*`, `general.screenshot_resize`): https://www.controlid.com.br/docs/access-api-en/configurations/configuration-parameters/

## Resumo executivo

- Catálogo oficial da PoC: 96 entradas.
- Endpoints invocáveis no catálogo: 73.
- Callbacks e rotas de servidor no catálogo: 23.
- Endpoints oficiais consumidos diretamente por controllers e telas dedicadas: 67.

## Status geral

### Cobertura forte

- Sessão: login, logout, validação de sessão e troca de credenciais do equipamento.
- Objetos: `load_objects`, `create_objects`, `modify_objects` e `destroy_objects`, com fluxos dedicados para usuários, grupos, cartões, QR codes, regras de acesso, templates biométricos, dispositivos, configurações, logs e eventos de catraca.
- Ações: autorização remota, execução de ações remotas, buzzer, mensagem em tela e cadastro remoto.
- Fotos, logo e vídeo: `user_get_image`, `user_list_images`, `user_get_image_list`, `user_set_image`, `user_set_image_list`, `user_test_image`, `user_destroy_image`, `logo.fcgi`, `logo_change.fcgi`, `logo_destroy.fcgi`, `send_video.fcgi`, `set_custom_video.fcgi`, `remove_custom_video.fcgi` e `save_screenshot.fcgi`.
- Sistema e hardware: `system_information`, `hash-password`, `set_system_time`, `set_system_network`, OpenVPN, `gpio_state`, `reread_leds_settings`, `is_valid_biometry`, `remote_led_control.fcgi` e `set_network_interlock.fcgi`.
- Modos online, monitor e push: callbacks oficiais, monitor persistido, compatibilidade com webhook legado e central push oficial.
- Exportação e relatórios: `export_objects.fcgi`, `report_generate.fcgi`, `export_afd.fcgi` e `export_audit_logs.fcgi`.
- Recursos específicos de produto: upgrades de licença, streaming RTSP/ONVIF, configuração facial/documentada, QR Code/TOTP, energia e screenshot do iDFace Max, SIP intercom, toques SIP, mensagens de áudio do iDFace e sinais customizados do iDFace Max.
- Lista oficial de objetos: agora existe um módulo dedicado para toda a página `List of Objects`, cobrindo os objetos documentados com `load_objects`, `create_objects`, `create_or_modify_objects`, `modify_objects` e `destroy_objects`.

### Lacunas objetivas encontradas

- Nenhuma lacuna objetiva permaneceu aberta na revisão viva realizada nesta passada.
- O bloco novo desta rodada fechou explicitamente `create_or_modify_objects.fcgi`, a cobertura dedicada da página `List of Objects` e os tópicos documentados de QR Code/TOTP e energia/screenshot.

## Observações importantes

- Nem toda diferença em relação ao índice oficial representa um endpoint inédito. Vários tópicos atuais da documentação descrevem o uso de `set_configuration.fcgi` ou `get_configuration.fcgi`, e a PoC cobre esses dois endpoints tanto de forma genérica quanto com formulários dedicados para os grupos relevantes, incluindo attendance, online, visitors, hardening, SNMP, iDCloud, alarmes, QR/TOTP, energia, streaming, facial e sinais customizados.
- A PoC implementa rotas locais adicionais de compatibilidade para callbacks como `new_rex_log.fcgi` e `fingerprint_create.fcgi`, mesmo quando eles não aparecem como entradas explícitas na tela de invocação manual.
- A completude auditada aqui vale para o conjunto de páginas oficiais revisadas nesta data e para o smoke local com stub. A validação em hardware ou firmware real continua sendo a camada final para afirmar completude operacional absoluta.
- O smoke test local passou a executar 340 verificações com 310 PASS, 0 FAIL e 30 SKIP, cobrindo catálogo oficial, callbacks e telas dedicadas da PoC.

## Conclusão

No estado auditado e atualizado em 2026-04-13, a PoC passou a cobrir o conjunto de endpoints objetivos e dos tópicos documentados revisados nesta rodada, incluindo a lista oficial de objetos, `create_or_modify_objects.fcgi`, QR Code/TOTP e energia/screenshot do iDFace Max. A ressalva honesta permanece a mesma: completude absoluta, em sentido operacional, depende de validação em hardware ou firmware real e de nova revisão quando a documentação oficial viva mudar.
