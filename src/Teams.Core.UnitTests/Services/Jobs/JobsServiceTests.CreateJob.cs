// using Teams.Core.Exceptions;
// using Teams.Core.Services.Jobs.Requests;
// using Teams.Domain.Entities;
//
// namespace Teams.Core.UnitTests.Services.Jobs;
//
// public static partial class JobsServiceTests
// {
//     public class CreateJobAsync : JobsServiceTestsBase
//     {
//         [Fact]
//         public async Task PropagatesValidationException_WhenValidationFails()
//         {
//             var sut = CreateSut();
//             var request = new CreateJobRequest("idempotency-key-001", "ArchiveProjectJob", null);
//             Validator.ValidateCommandAsync(request, Arg.Any<CancellationToken>())
//                 .Returns(Task.FromException(new CommandValidationException([])));
//
//             await Assert.ThrowsAsync<CommandValidationException>(() =>
//                 sut.CreateJobAsync(request, TestContext.Current.CancellationToken));
//         }
//
//         [Fact]
//         public async Task ReturnsExistingJob_WhenIdempotencyKeyAlreadyExists()
//         {
//             var sut = CreateSut();
//             var request = new CreateJobRequest("idempotency-key-001", "ArchiveProjectJob", null);
//             var extant = CreateJob(request.IdempotencyKey);
//             JobsRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, Arg.Any<CancellationToken>())
//                 .Returns(extant);
//
//             var result = await sut.CreateJobAsync(request, TestContext.Current.CancellationToken);
//
//             Assert.Equal(extant.Id, result.Id);
//         }
//
//         [Fact]
//         public async Task DoesNotCreateNewJob_WhenIdempotencyKeyAlreadyExists()
//         {
//             var sut = CreateSut();
//             var request = new CreateJobRequest("idempotency-key-001", "ArchiveProjectJob", null);
//             var extant = CreateJob(request.IdempotencyKey);
//             JobsRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, Arg.Any<CancellationToken>())
//                 .Returns(extant);
//
//             await sut.CreateJobAsync(request, TestContext.Current.CancellationToken);
//
//             await JobsRepository.DidNotReceive().CreateAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>());
//             await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
//         }
//
//         [Fact]
//         public async Task CreatesAndSavesNewJob_WhenIdempotencyKeyDoesNotExist()
//         {
//             var sut = CreateSut();
//             var request = new CreateJobRequest("idempotency-key-001", "ArchiveProjectJob", null);
//             JobsRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, Arg.Any<CancellationToken>())
//                 .Returns((Job?)null);
//             JobsRepository.CreateAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>())
//                 .Returns(callInfo => Task.FromResult(callInfo.Arg<Job>()!));
//
//             var result = await sut.CreateJobAsync(request, TestContext.Current.CancellationToken);
//
//             await JobsRepository.Received(1).CreateAsync(
//                 Arg.Is<Job>(job => job!.IdempotencyKey == request.IdempotencyKey),
//                 Arg.Any<CancellationToken>());
//             await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
//             Assert.Equal(request.IdempotencyKey, result.IdempotencyKey);
//         }
//     }
// }