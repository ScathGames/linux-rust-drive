using ProtonDrive.Shared;

namespace ProtonDrive.App;

public sealed record AppArguments(AppLaunchMode LaunchMode, AppCrashMode CrashMode);
