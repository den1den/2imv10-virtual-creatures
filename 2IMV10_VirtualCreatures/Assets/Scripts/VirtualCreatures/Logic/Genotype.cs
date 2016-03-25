using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>
    /// The Genotype of a creature
    /// out of the scope of this project, not enough time left
    /// </summary>
    public class Genotype
    {
        public NNSpecification brain = null;
        public IList<EdgeGen> edges = null;
        public IList<Node> nodes = null;
        public Node root = null;

        public Genotype() { }
    }
}

