﻿using GShark.Core;

namespace GShark.Interfaces
{
    public interface ITransformable<T>
    {
        public T Transform(TransformMatrix t);
    }
}