using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Выбираем только безопасные данные, пароли на фронтенд не отправляем
        var users = await _context.Users
            .Select(u => new
            {
                u.UserId,
                u.FullName,
                u.Login,
                u.Email,
                u.RoleId
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Identifier))
            return BadRequest("Пустой запрос");

        // Ищем пользователя: проверяем совпадение Identifier с Login ИЛИ с Email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Login == loginRequest.Identifier || u.Email == loginRequest.Identifier)
                                       && u.Password == loginRequest.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Неверные данные для входа" });
        }

        // Возвращаем объект, который MAUI сможет прочитать
        return Ok(new
        {
            user.UserId,
            user.FullName,
            user.Login,
            user.Email,
            user.RoleId
        });
    }

    // ВХОД ТОЛЬКО ДЛЯ АДМИНИСТРАТОРОВ (Для React-панели)
    // Жестко проверяет, что RoleId равен 1
    [HttpPost("loginAdmin")]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Identifier))
            return BadRequest("Пустой запрос");

        // Ищем пользователя по логину/email и паролю
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Login == loginRequest.Identifier || u.Email == loginRequest.Identifier)
                                       && u.Password == loginRequest.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Неверные данные для входа" });
        }

        // НАДЕЖНАЯ ПРОВЕРКА: Смотрим на RoleId напрямую из таблицы Users. 
        // У joil в базе стоит RoleId = 1, значит 1 — это админ.
        if (user.RoleId != 1 && user.RoleId != 4)
        {
            return StatusCode(403, new { message = "Доступ запрещен. Вы не являетесь администратором." });
        }

        return Ok(new
        {
            user.UserId,
            user.FullName,
            user.Login,
            user.Email,
            user.RoleId
        });
    }

    // POST: api/Users/register
    // Открытая регистрация для мобильного приложения (обычные пользователи)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { message = "Все поля (Логин, Email, Пароль) обязательны для заполнения." });
        }

        // Проверяем уникальность
        var isDuplicate = await _context.Users
            .AnyAsync(u => u.Login == request.Login || u.Email == request.Email);

        if (isDuplicate)
        {
            return BadRequest(new { message = "Пользователь с таким Логином или Email уже существует." });
        }

        // Создаем пользователя со стандартной ролью (например, 2 - Покупатель)
        var newUser = new User
        {
            FullName = request.FullName,
            Login = request.Login,
            Email = request.Email,
            Password = request.Password, // Пароль в исходном виде, согласно вашей архитектуре
            RoleId = 2
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Возвращаем объект пользователя, чтобы MAUI мог сразу его авторизовать при желании
        return Ok(new
        {
            newUser.UserId,
            newUser.FullName,
            newUser.Login,
            newUser.Email,
            newUser.RoleId
        });
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // Ищем пользователя в базе данных по ID
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        try
        {
            // Удаляем пользователя
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пользователь успешно удален" });
        }
        catch (DbUpdateException ex)
        {
            // На случай, если у пользователя есть связанные заказы или отзывы (ошибка внешнего ключа)
            return BadRequest(new { message = "Невозможно удалить пользователя, так как с ним связаны другие данные (заказы, отзывы)." });
        }
    }

    // GET: api/Users/roles
    // Получение списка всех доступных ролей для выпадающего списка в React
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        // Предполагаем, что у вас в AppDbContext есть DbSet<Role> (исходя из прошлых Include)
        var roles = await _context.Roles
            .Select(r => new
            {
                r.RoleId,
                r.Name
            })
            .ToListAsync();

        return Ok(roles);
    }

    // POST: api/Users
    // Создание нового пользователя администратором
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Логин и пароль обязательны для заполнения." });

        // Проверяем, нет ли уже пользователя с таким же Логином или Email
        var isDuplicate = await _context.Users
            .AnyAsync(u => u.Login == request.Login || u.Email == request.Email);

        if (isDuplicate)
        {
            return BadRequest(new { message = "Пользователь с таким Логином или Email уже существует в системе." });
        }

        // Создаем новый объект пользователя
        // Замените "User" на точное имя вашего класса сущности (например, UserEntity, если оно отличается)
        var newUser = new User
        {
            FullName = request.FullName,
            Login = request.Login,
            Email = request.Email,
            Password = request.Password, // Пароль сохраняется в исходном виде, как у вас в Login
            RoleId = request.RoleId
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь успешно создан!" });
    }

    // PUT: api/Users/update-profile
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });

        // Проверяем, не занят ли новый логин или email кем-то другим
        var isDuplicate = await _context.Users
            .AnyAsync(u => u.UserId != request.UserId && (u.Login == request.Login || u.Email == request.Email));

        if (isDuplicate)
        {
            return BadRequest(new { message = "Этот Логин или Email уже заняты другим пользователем." });
        }

        // Обновляем данные
        user.FullName = request.FullName;
        user.Login = request.Login;
        user.Email = request.Email;

        await _context.SaveChangesAsync();

        // Возвращаем обновленный объект для React
        return Ok(new
        {
            user.UserId,
            user.FullName,
            user.Login,
            user.Email,
            user.RoleId
        });
    }

    // PUT: api/Users/change-password
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });

        // Проверяем старый пароль (сверяем напрямую, так как они в чистом виде)
        if (user.Password != request.CurrentPassword)
        {
            return BadRequest(new { message = "Текущий пароль указан неверно." });
        }

        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 4)
        {
            return BadRequest(new { message = "Новый пароль слишком короткий (минимум 4 символа)." });
        }

        // Сохраняем новый пароль
        user.Password = request.NewPassword;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пароль успешно изменен!" });
    }

    // GET: api/Users/{id}/stats
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetUserStats(int id)
    {
        // Проверяем существование пользователя
        var userExists = await _context.Users.AnyAsync(u => u.UserId == id);
        if (!userExists) return NotFound(new { message = "Пользователь не найден" });

        // Загружаем все заказы этого пользователя
        var userOrders = await _context.Orders
            .Where(o => o.UserId == id)
            .ToListAsync();

        // Считаем агрегированные метрики по заказам
        int totalOrders = userOrders.Count;
        decimal totalRevenue = userOrders.Sum(o => o.Amount);
        decimal avgOrderAmount = totalOrders > 0 ? userOrders.Average(o => o.Amount) : 0;
        DateTime? lastOrderDate = totalOrders > 0 ? userOrders.Max(o => o.OrderDate) : null;

        // Считаем количество написанных отзывов из таблицы Reviews
        int totalReviews = await _context.Reviews.CountAsync(r => r.UserId == id);

        // Формируем детальную историю заказов для вывода в таблицу
        var orderHistory = userOrders
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new {
                o.OrderId,
                o.OrderDate,
                o.Amount,
                o.Status
            })
            .ToList();

        return Ok(new
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            AvgOrderAmount = Math.Round(avgOrderAmount, 2),
            LastOrderDate = lastOrderDate,
            TotalReviews = totalReviews,
            OrderHistory = orderHistory
        });
    }

   // Добавьте эту DTO модель в нижнюю часть контроллера к остальным классам
    public class RegisterUserRequest
    {
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO модели запросов
    public class UpdateProfileRequest
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    // Модель для принятия данных с фронтенда
    public class CreateUserRequest
    {
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }

    public class LoginRequest
    {
        // Называем поле Identifier, так как там может быть и то, и другое
        public string Identifier { get; set; }
        public string Password { get; set; }
    }
}