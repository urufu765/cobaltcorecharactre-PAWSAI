using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nickel;
// using JollyJolly.Actions;
// using JollyJolly.Cards;


namespace JollyJolly.Artifacts;

public class DuoArtifactMeta : Attribute
{
    public Deck duoDeck;
    public string? duoModDeck;
}