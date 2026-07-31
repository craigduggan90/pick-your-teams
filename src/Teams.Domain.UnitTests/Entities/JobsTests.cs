// using Teams.Common.Providers.Identifiers;
// using Teams.Common.Providers.Temporal;
// using Teams.Domain.Entities;
// using Teams.Domain.Enums;
//
// namespace Teams.Domain.UnitTests.Entities;
//
// public static class JobTests
// {
//     public abstract class JobTestsBase
//     {
//         protected const string DefaultIdempotencyKey = "idempotency-key-001";
//         protected const JobTypeEnum DefaultType = JobTypeEnum.ArchiveProjectJob;
//         protected const string DefaultParameters = """{"foo":"bar"}""";
//
//         protected static Job CreateJob(Action<Job>? setup = null)
//         {
//             var job = new Job(DefaultIdempotencyKey, DefaultType, DefaultParameters);
//             setup?.Invoke(job);
//             return job;
//         }
//     }
//
//     public class Constructor : JobTestsBase
//     {
//         [Fact]
//         public void SetsIdempotencyKeyTypeAndParameters_WhenConstructed()
//         {
//             var job = new Job(DefaultIdempotencyKey, DefaultType, DefaultParameters);
//
//             Assert.Equal(DefaultIdempotencyKey, job.IdempotencyKey);
//             Assert.Equal(DefaultType, job.Type);
//             Assert.Equal(DefaultParameters, job.Parameters);
//         }
//
//         [Fact]
//         public void SetsStatusToPending_WhenConstructed()
//         {
//             var job = CreateJob();
//
//             Assert.Equal(JobStatusEnum.Pending, job.Status);
//         }
//
//         [Fact]
//         public void SetsErrorFieldsToNull_WhenConstructed()
//         {
//             var job = CreateJob();
//
//             Assert.Null(job.ErrorCode);
//             Assert.Null(job.ErrorMessage);
//         }
//
//         [Fact]
//         public void SetsParametersToNull_WhenParametersArgumentIsNull()
//         {
//             var job = new Job(DefaultIdempotencyKey, DefaultType, null);
//
//             Assert.Null(job.Parameters);
//         }
//
//         [Fact]
//         public void SetsId_WhenIdentifierProviderIsFixed()
//         {
//             using var _ = new IdentifierProviderContext("fixed-id");
//
//             var job = CreateJob();
//
//             Assert.Equal("fixed-id", job.Id);
//         }
//
//         [Fact]
//         public void SetsDateCreatedAndDateModified_WhenTimeProviderIsFixed()
//         {
//             var fixedTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
//             using var _ = new DateTimeOffsetProviderContext(fixedTime);
//
//             var job = CreateJob();
//
//             Assert.Equal(fixedTime.UtcDateTime, job.DateCreated);
//             Assert.Equal(fixedTime.UtcDateTime, job.DateModified);
//         }
//
//         [Fact]
//         public void SetsConcurrencyToken_WhenConstructed()
//         {
//             var job = CreateJob();
//
//             Assert.False(string.IsNullOrEmpty(job.ConcurrencyToken));
//         }
//     }
//
//     public class Update : JobTestsBase
//     {
//         [Fact]
//         public void SetsStatus_WhenCalled()
//         {
//             var job = CreateJob();
//
//             job.Update(JobStatusEnum.InProgress, null, null);
//
//             Assert.Equal(JobStatusEnum.InProgress, job.Status);
//         }
//
//         [Fact]
//         public void SetsErrorCodeAndMessage_WhenStatusIsFailed()
//         {
//             var job = CreateJob();
//
//             job.Update(JobStatusEnum.Failed, "SOME_CODE", "some message");
//
//             Assert.Equal(JobStatusEnum.Failed, job.Status);
//             Assert.Equal("SOME_CODE", job.ErrorCode);
//             Assert.Equal("some message", job.ErrorMessage);
//         }
//
//         [Fact]
//         public void MarksJobAsDirty_WhenValueChanges()
//         {
//             var job = CreateJob();
//
//             job.Update(JobStatusEnum.InProgress, null, null);
//
//             Assert.True(job.IsDirty);
//         }
//
//         [Fact]
//         public void DoesNotMarkJobAsDirty_WhenValuesAreUnchanged()
//         {
//             var job = CreateJob();
//
//             job.Update(JobStatusEnum.Pending, null, null);
//
//             Assert.False(job.IsDirty);
//         }
//
//         [Fact]
//         public void ChangesConcurrencyToken_WhenValueChanges()
//         {
//             var job = CreateJob();
//             var initialToken = job.ConcurrencyToken;
//
//             job.Update(JobStatusEnum.InProgress, null, null);
//
//             Assert.NotEqual(initialToken, job.ConcurrencyToken);
//         }
//
//         [Fact]
//         public void DoesNotChangeConcurrencyToken_WhenValuesAreUnchanged()
//         {
//             var job = CreateJob();
//             var initialToken = job.ConcurrencyToken;
//
//             job.Update(JobStatusEnum.Pending, null, null);
//
//             Assert.Equal(initialToken, job.ConcurrencyToken);
//         }
//
//         [Fact]
//         public void AllowsStatusToBeAppliedRepeatedly_WhenCalledMultipleTimes()
//         {
//             var job = CreateJob();
//
//             job.Update(JobStatusEnum.InProgress, null, null);
//             job.Update(JobStatusEnum.Complete, null, null);
//
//             Assert.Equal(JobStatusEnum.Complete, job.Status);
//         }
//
//         [Fact]
//         public void DoesNotChangeStatus_WhenSameStatusIsProvidedAgain()
//         {
//             var job = CreateJob();
//             job.Update(JobStatusEnum.Failed, "SOME_CODE", "some message");
//
//             job.Update(JobStatusEnum.Failed, "OTHER_CODE", "some other message");
//
//             Assert.Equal(JobStatusEnum.Failed, job.Status);
//             Assert.Equal("OTHER_CODE", job.ErrorCode);
//             Assert.Equal("some other message", job.ErrorMessage);
//         }
//
//         [Fact]
//         public void MarksJobAsDirty_WhenOnlyErrorFieldsChange_EvenIfStatusIsUnchanged()
//         {
//             var job = CreateJob();
//             job.Update(JobStatusEnum.Failed, "SOME_CODE", "some message");
//
//             job.Update(JobStatusEnum.Failed, "OTHER_CODE", "some other message");
//
//             Assert.True(job.IsDirty);
//         }
//     }
//
//     public class Delete : JobTestsBase
//     {
//         [Fact]
//         public void SetsDateDeleted_WhenCalled()
//         {
//             var fixedTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
//             using var _ = new DateTimeOffsetProviderContext(fixedTime);
//             var job = CreateJob();
//
//             job.Delete();
//
//             Assert.Equal(fixedTime.UtcDateTime, job.DateDeleted);
//         }
//
//         [Fact]
//         public void MarksJobAsDirty_WhenCalled()
//         {
//             var job = CreateJob();
//
//             job.Delete();
//
//             Assert.True(job.IsDirty);
//         }
//
//         [Fact]
//         public void ChangesConcurrencyToken_WhenCalled()
//         {
//             var job = CreateJob();
//             var initialToken = job.ConcurrencyToken;
//
//             job.Delete();
//
//             Assert.NotEqual(initialToken, job.ConcurrencyToken);
//         }
//
//         [Fact]
//         public void DoesNotChangeDateDeleted_WhenCalledTwice()
//         {
//             using var firstDeleteTime = new DateTimeOffsetProviderContext(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
//             var job = CreateJob(j => j.Delete());
//             var firstDeletedAt = job.DateDeleted;
//
//             using var secondDeleteTime = new DateTimeOffsetProviderContext(new DateTimeOffset(2026, 1, 1, 12, 0, 1, TimeSpan.Zero));
//             job.Delete();
//
//             Assert.Equal(firstDeletedAt, job.DateDeleted);
//         }
//
//         [Fact]
//         public void DoesNotChangeConcurrencyToken_WhenCalledTwice()
//         {
//             var job = CreateJob(j => j.Delete());
//             var tokenAfterFirstDelete = job.ConcurrencyToken;
//
//             job.Delete();
//
//             Assert.Equal(tokenAfterFirstDelete, job.ConcurrencyToken);
//         }
//     }
//
//     public class AsSerializableTests : JobTestsBase
//     {
//         [Fact]
//         public void DoesNotIncludeParametersOrErrorMessage_WhenCalled()
//         {
//             var job = CreateJob();
//
//             var serializable = job.AsSerializable();
//             var type = serializable.GetType();
//
//             Assert.Null(type.GetProperty("Parameters"));
//             Assert.Null(type.GetProperty("ErrorMessage"));
//         }
//
//         [Fact]
//         public void ReflectsIdIdempotencyKeyTypeAndStatus_WhenCalled()
//         {
//             var job = CreateJob();
//
//             var serializable = job.AsSerializable();
//             var type = serializable.GetType();
//
//             Assert.Equal(job.Id, type.GetProperty("Id")!.GetValue(serializable));
//             Assert.Equal(job.IdempotencyKey, type.GetProperty("IdempotencyKey")!.GetValue(serializable));
//             Assert.Equal(job.Type, type.GetProperty("Type")!.GetValue(serializable));
//             Assert.Equal(job.Status, type.GetProperty("Status")!.GetValue(serializable));
//         }
//
//         [Fact]
//         public void ReflectsErrorCode_WhenJobHasFailed()
//         {
//             var job = CreateJob(j => j.Update(JobStatusEnum.Failed, "SOME_CODE", "some message"));
//
//             var serializable = job.AsSerializable();
//             var errorCode = serializable.GetType().GetProperty("ErrorCode")!.GetValue(serializable);
//
//             Assert.Equal("SOME_CODE", errorCode);
//         }
//
//         [Fact]
//         public void ReflectsErrorCodeAsNull_WhenJobHasNotFailed()
//         {
//             var job = CreateJob();
//
//             var serializable = job.AsSerializable();
//             var errorCode = serializable.GetType().GetProperty("ErrorCode")!.GetValue(serializable);
//
//             Assert.Null(errorCode);
//         }
//
//         [Fact]
//         public void ReflectsDateCreatedAndDateModified_WhenTimeProviderIsFixed()
//         {
//             var fixedTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
//             using var _ = new DateTimeOffsetProviderContext(fixedTime);
//             var job = CreateJob();
//
//             var serializable = job.AsSerializable();
//             var type = serializable.GetType();
//
//             Assert.Equal(job.DateCreated, type.GetProperty("DateCreated")!.GetValue(serializable));
//             Assert.Equal(job.DateModified, type.GetProperty("DateModified")!.GetValue(serializable));
//         }
//     }
// }