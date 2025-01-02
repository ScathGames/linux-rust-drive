namespace ProtonDrive.App.Onboarding;

public interface IOnboardingStateAware
{
    void OnboardingStateChanged(OnboardingState value);
}
