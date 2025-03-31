namespace ProtonDrive.App.Onboarding;

public interface IStorageOptimizationOnboardingStateAware
{
    void StorageOptimizationOnboardingStateChanged(StorageOptimizationOnboardingStep value);
}
