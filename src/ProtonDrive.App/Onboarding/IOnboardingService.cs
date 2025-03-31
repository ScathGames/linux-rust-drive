namespace ProtonDrive.App.Onboarding;

public interface IOnboardingService
{
    void CompleteStep(OnboardingStep step);
    void CompleteSharedWithMeOnboarding();
    void CompleteStorageOptimizationOnboardingStep(StorageOptimizationOnboardingStep step);
}
