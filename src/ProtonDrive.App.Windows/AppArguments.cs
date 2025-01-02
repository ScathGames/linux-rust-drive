using ProtonDrive.Shared;

namespace ProtonDrive.App.Windows;

internal sealed record AppArguments(AppLaunchMode LaunchMode, AppCrashMode CrashMode);
