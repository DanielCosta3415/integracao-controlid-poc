using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.Users;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class UsersController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<UsersController> _logger;

        public UsersController(OfficialControlIdApiService officialApi, ILogger<UsersController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new UserListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Users = (await LoadUsersAsync())
                    .Select(ToUserViewModel)
                    .OrderBy(user => user.Name)
                    .ToList();

                if (model.Users.Count == 0)
                    model.ErrorMessage = "Nenhum usuário encontrado.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar usuários", ex);
                _logger.LogError(ex, "Erro ao consultar usuários.");
            }

            return View(model);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var user = (await LoadUsersAsync(id.Value)).FirstOrDefault();
                if (user != null)
                    return View(ToUserViewModel(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do usuário {UserId}.", id.Value);
            }

            return NotFound();
        }

        public IActionResult Create()
        {
            return View(new UserEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var values = await BuildUserValuesAsync(model);
                var result = await _officialApi.InvokeAsync("create-objects", new
                {
                    @object = "users",
                    values = new[] { values }
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao criar usuário", result));
                    return View(model);
                }

                TempData["StatusMessage"] = "Usuário criado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("Erro ao criar usuário", ex));
                _logger.LogError(ex, "Erro ao criar usuário.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var user = (await LoadUsersAsync(id.Value)).FirstOrDefault();
                if (user != null)
                    return View(ToEditViewModel(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário {UserId} para edição.", id.Value);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, UserEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var values = await BuildUserValuesAsync(model);
                var result = await _officialApi.InvokeAsync("modify-objects", new
                {
                    @object = "users",
                    values,
                    where = new { users = new { id } }
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao atualizar usuário", result));
                    return View(model);
                }

                TempData["StatusMessage"] = "Usuário atualizado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("Erro ao atualizar usuário", ex));
                _logger.LogError(ex, "Erro ao atualizar usuário {UserId}.", id);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var user = (await LoadUsersAsync(id.Value)).FirstOrDefault();
                if (user != null)
                {
                    return View(new UserDeleteViewModel
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Registration = user.Registration
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário {UserId} para exclusão.", id.Value);
            }

            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (!_officialApi.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _officialApi.InvokeAsync("destroy-objects", new
                {
                    @object = "users",
                    where = new { users = new { id } }
                });

                TempData["StatusMessage"] = result.Success
                    ? "Usuário excluído com sucesso!"
                    : BuildErrorMessage("Erro ao excluir usuário", result);
                TempData["StatusType"] = result.Success ? "success" : "danger";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao excluir usuário", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir usuário {UserId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<UserRecord>> LoadUsersAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new { @object = "users", where = new { users = new { id = id.Value } } }
                : new { @object = "users" };

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao consultar usuários", result));

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "users")
                .Select(MapUser)
                .ToList();
        }

        private async Task<Dictionary<string, object?>> BuildUserValuesAsync(UserEditViewModel model)
        {
            var values = new Dictionary<string, object?>
            {
                ["registration"] = model.Registration,
                ["name"] = model.Name,
                ["user_type_id"] = model.UserTypeId,
                ["begin_time"] = ToUnixTime(model.BeginTime),
                ["end_time"] = ToUnixTime(model.EndTime)
            };

            if (!string.IsNullOrWhiteSpace(model.Email))
                values["email"] = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Phone))
                values["phone"] = model.Phone;

            if (!string.IsNullOrWhiteSpace(model.Status))
                values["status"] = model.Status;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var hash = await GeneratePasswordHashAsync(model.Password);
                values["password"] = hash.Password;
                values["salt"] = hash.Salt;
            }

            return values;
        }

        private async Task<Integracao.ControlID.PoC.Models.ControlIDApi.HashPasswordResponse> GeneratePasswordHashAsync(string password)
        {
            var (result, document) = await _officialApi.InvokeJsonAsync("hash-password", new { password });
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao gerar hash da senha", result));

            if (document == null)
                throw new InvalidOperationException("Resposta inesperada ao gerar hash da senha.");

            var hash = JsonSerializer.Deserialize<Integracao.ControlID.PoC.Models.ControlIDApi.HashPasswordResponse>(document.RootElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (hash == null || string.IsNullOrWhiteSpace(hash.Password) || string.IsNullOrWhiteSpace(hash.Salt))
                throw new InvalidOperationException("Resposta inesperada ao gerar hash da senha.");

            return hash;
        }

        private static UserRecord MapUser(JsonElement element)
        {
            return new UserRecord
            {
                Id = GetInt64(element, "id"),
                Registration = GetString(element, "registration") ?? string.Empty,
                Name = GetString(element, "name") ?? string.Empty,
                Email = GetString(element, "email"),
                Phone = GetString(element, "phone"),
                Status = GetString(element, "status"),
                UserTypeId = GetInt32(element, "user_type_id", "userTypeId"),
                BeginTime = GetNullableInt64(element, "begin_time", "beginTime"),
                EndTime = GetNullableInt64(element, "end_time", "endTime"),
                CreatedAt = GetDateTime(element, "created_at", "createdAt"),
                UpdatedAt = GetDateTime(element, "updated_at", "updatedAt")
            };
        }

        private static UserViewModel ToUserViewModel(UserRecord user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Registration = user.Registration,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Status = user.Status ?? string.Empty,
                BeginTime = FromUnixTime(user.BeginTime),
                EndTime = FromUnixTime(user.EndTime),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        private static UserEditViewModel ToEditViewModel(UserRecord user)
        {
            return new UserEditViewModel
            {
                Id = user.Id,
                Registration = user.Registration,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                Status = user.Status ?? string.Empty,
                UserTypeId = user.UserTypeId,
                BeginTime = FromUnixTime(user.BeginTime),
                EndTime = FromUnixTime(user.EndTime)
            };
        }

        private static IEnumerable<JsonElement> ExtractArray(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out var array) && array.ValueKind == JsonValueKind.Array)
                return array.EnumerateArray();

            return [];
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string? GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                    return property.GetString();
            }

            return null;
        }

        private static long GetInt64(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var number))
                        return number;

                    if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out number))
                        return number;
                }
            }

            return 0;
        }

        private static long? GetNullableInt64(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var number))
                        return number;

                    if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out number))
                        return number;
                }
            }

            return null;
        }

        private static int GetInt32(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
                        return number;

                    if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out number))
                        return number;
                }
            }

            return 0;
        }

        private static DateTime? GetDateTime(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) &&
                    property.ValueKind == JsonValueKind.String &&
                    DateTime.TryParse(property.GetString(), out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static DateTime? FromUnixTime(long? value)
        {
            return value.HasValue ? DateTimeOffset.FromUnixTimeSeconds(value.Value).LocalDateTime : null;
        }

        private static long? ToUnixTime(DateTime? value)
        {
            return value.HasValue ? new DateTimeOffset(value.Value).ToUnixTimeSeconds() : null;
        }

        private sealed class UserRecord
        {
            public long Id { get; init; }
            public string Registration { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public string? Email { get; init; }
            public string? Phone { get; init; }
            public string? Status { get; init; }
            public int UserTypeId { get; init; }
            public long? BeginTime { get; init; }
            public long? EndTime { get; init; }
            public DateTime? CreatedAt { get; init; }
            public DateTime? UpdatedAt { get; init; }
        }
    }
}




