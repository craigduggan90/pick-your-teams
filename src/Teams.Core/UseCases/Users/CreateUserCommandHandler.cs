// using Teams.Core.CQRS;
// using Teams.Core.Services.IdentityProvider;
// using Teams.Data.Services;
// using Teams.Domain.Entities;
//
// namespace Teams.Core.UseCases.Users;
//
//
// public class CreateUserCommandHandler(
//     IIdpClient idp, 
//     IUnitOfWork uow) 
//     : IRequestHandler<CreateUserCommand, User>
// {
//     public async Task<User> HandleAsync(CreateUserCommand request, CancellationToken cancellationToken)
//     {
//         var player = await uow.Users.CreateAsync(new User(
//             request.Tag, 
//             request.DisplayName,
//             request.Email, 
//             request.Mobile), cancellationToken);
//
//         var externalId = await idp.CreateUser(request.Tag, request.Email, request.Mobile, cancellationToken);
//         player.SetExternalId(externalId);
//         
//         await uow.SaveChangesAsync(cancellationToken);
//         return player;
//     }
// }