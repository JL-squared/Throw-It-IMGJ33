using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceHealth : EntityHealth {
    public List<Piece> supportingPieces; // can it balance?
    public List<Piece> loadPieces; // will it break
    public Vector2 loadCenter; // center of mass for this piece and everything it supports
    public Vector2 supportCenter;
}
