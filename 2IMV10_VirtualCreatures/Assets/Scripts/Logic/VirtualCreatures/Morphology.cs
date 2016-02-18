using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.Logic.VCGenotype;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    class Morphology
    {
        public NNSpecification brain;
        public IList<Edge> edges;
        public IList<Node> nodes;
        public Node root;
        public Genotype genotype;

        public Morphology(Node root, NNSpecification brain, IList<Edge> edges, Genotype genotype)
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
