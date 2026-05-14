using System;

namespace boilersGraphics.Models.Animation;

public readonly record struct PropertyRef(
    Guid ItemId,
    string PropertyPath,
    AnimatedValueType ValueType);
