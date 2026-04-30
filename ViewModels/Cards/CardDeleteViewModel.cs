namespace Integracao.ControlID.PoC.ViewModels.Cards
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de cartão.
    /// </summary>
    public class CardDeleteViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
