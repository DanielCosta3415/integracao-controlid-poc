using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Monitor
{
    public class MonitorEventQueue
    {
        private readonly ConcurrentQueue<MonitorEvent> _queue = new ConcurrentQueue<MonitorEvent>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        /// <summary>
        /// Adiciona um evento à fila.
        /// </summary>
        public void Enqueue(MonitorEvent evt)
        {
            _queue.Enqueue(evt);
            _signal.Release();
        }

        /// <summary>
        /// Aguarda e retorna o próximo evento da fila.
        /// </summary>
        public async Task<MonitorEvent> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _queue.TryDequeue(out var evt);
            return evt!;
        }

        /// <summary>
        /// Retorna o número de eventos na fila.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Remove todos os eventos da fila.
        /// </summary>
        public void Clear()
        {
            while (_queue.TryDequeue(out _)) { }
        }
    }
}
