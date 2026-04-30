using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Data; // Contexto correto
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Database
{
    public class CardRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<CardRepository> _logger;

        public CardRepository(IntegracaoControlIDContext dbContext, ILogger<CardRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo cartão local.
        /// </summary>
        public async Task<CardLocal> AddCardAsync(CardLocal card)
        {
            try
            {
                card.CreatedAt = DateTime.UtcNow;
                _dbContext.Cards.Add(card);
                await _dbContext.SaveChangesAsync();
                return card;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar cartão local.");
                throw;
            }
        }

        /// <summary>
        /// Busca cartão local pelo Id.
        /// </summary>
        public async Task<CardLocal?> GetCardByIdAsync(long id)
        {
            return await _dbContext.Cards.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os cartões locais.
        /// </summary>
        public async Task<List<CardLocal>> GetAllCardsAsync()
        {
            return await _dbContext.Cards.OrderBy(c => c.Id).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um cartão local.
        /// </summary>
        public async Task<bool> UpdateCardAsync(CardLocal card)
        {
            try
            {
                _dbContext.Cards.Update(card);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar cartão local {card.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove cartão local pelo Id.
        /// </summary>
        public async Task<bool> DeleteCardAsync(long id)
        {
            try
            {
                var card = await _dbContext.Cards.FindAsync(id);
                if (card == null)
                    return false;

                _dbContext.Cards.Remove(card);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover cartão local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca cartões locais por usuário ou status.
        /// </summary>
        public async Task<List<CardLocal>> SearchCardsAsync(long? userId = null, string? status = null)
        {
            IQueryable<CardLocal> query = _dbContext.Cards;

            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            return await query.OrderBy(c => c.Id).ToListAsync();
        }
    }
}
