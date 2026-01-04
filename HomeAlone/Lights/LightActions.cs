namespace HomeAlone.Lights;

internal enum LightActions : byte
{
    Off = 0,
    On = 1,
    Toggle = 2,
    Dim1 = 3,
    Dim2 = 4,
    BlinkAndOn = 5,
    BlinkAndOff = 6,
    BlinkAndOriginal = 7,
    OnPassiveInfraRed = 8,
}
