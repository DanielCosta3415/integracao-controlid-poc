using System;

namespace Integracao.ControlID.PoC.Services.OperationModes
{
    public sealed class OperationModesProfileResolver
    {
        /// <summary>
        /// Resolve o modo operacional a partir dos flags oficiais `online` e `local_identification`.
        /// </summary>
        /// <param name="onlineEnabled">Indica se o equipamento esta em modo online.</param>
        /// <param name="localIdentificationEnabled">Indica se a identificacao local permanece ativa.</param>
        /// <returns>Snapshot com chave, rotulo e descricao do modo detectado.</returns>
        public OperationModesProfileSnapshot Resolve(bool onlineEnabled, bool localIdentificationEnabled)
        {
            if (!onlineEnabled)
            {
                return new OperationModesProfileSnapshot(
                    "standalone",
                    "Standalone",
                    "Operação local no equipamento, sem depender de um servidor online para identificar ou autorizar.");
            }

            if (localIdentificationEnabled)
            {
                return new OperationModesProfileSnapshot(
                    "pro",
                    "Pro",
                    "Modo online com identificação local ativa e callbacks oficiais para consolidar eventos e sincronização.");
            }

            return new OperationModesProfileSnapshot(
                "enterprise",
                "Enterprise",
                "Modo online orientado a servidor, com identificação centralizada e fluxo expandido para integrações corporativas.");
        }
    }

    public sealed record OperationModesProfileSnapshot(string Key, string Label, string Description);
}
