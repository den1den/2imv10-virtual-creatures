﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Logic.Genotype
{
    public class Genotype
    {
        public NNSpecification brain;
        public IList<Edge> edges;
        public IList<Node> nodes;
        public Node root;

        public Genotype(Node root, NNSpecification brain, IList<Edge> edges)
        {
            IList<Node> nodes = edges.SelectMany(e => new Node[] { e.source, e.destination }).Distinct().ToList();
            if (!edges.Select(e => e.network).Contains(brain))
            {
                throw new ArgumentException();
            }
            this.root = root;
            this.brain = brain;
            this.edges = edges;
            this.nodes = nodes;
        }
    }
}

