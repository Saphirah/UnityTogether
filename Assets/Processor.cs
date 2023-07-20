using System;
using UnityEngine;

public abstract class Processor
{
    protected readonly UnityTogetherClient communication;
    protected Processor(UnityTogetherClient com) => communication = com;
}