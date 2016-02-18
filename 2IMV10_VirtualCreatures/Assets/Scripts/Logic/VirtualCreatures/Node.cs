using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    public class Node
    {
        public ShapeSpecification shape;
        public Node(ShapeSpecification shape)
        {
            this.shape = shape;
        }
    }
}
