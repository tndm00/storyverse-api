namespace Authentication.Application.Commands.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(ApplicationLogConstants.RegisterAttempt, request.Email);

        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning(ApplicationLogConstants.RegisterFailedEmailExists, request.Email);
            throw new ConflictException(ApplicationErrorConstants.EmailAlreadyRegistered);
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName
        };

        // Every account starts as a Reader; an author onboarding adds Author.
        user.Roles.Add(new UserRole { Role = Role.Reader });

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.RegisterSucceeded, user.Id);

        return user.Adapt<RegisterResponseDto>();
    }
}
