// State scale: higher value = more distressed.
// Empty is a special non-reactive state (Act 3+) that requires Emotional Burst to unlock.
// Overwhelmed is the threshold where Match-3 opens as a healing mini-game.
public enum EntityState
{
    Empty       = -1,
    Stable      =  0,
    Receptive   =  1,
    Overwhelmed =  2,
    Distressed  =  3,
    Agitated    =  4,
    Turbulent   =  5
}
