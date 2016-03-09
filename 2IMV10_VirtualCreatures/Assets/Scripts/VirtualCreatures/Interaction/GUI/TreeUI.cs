using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VirtualCreatures
{
    public class TreeUI : MonoBehaviour
    {

        public TreeNodeUI root;
        public float nodeXSpacing = 1.0f;
        public float nodeYSpacing = 1.0f;
        public float nodeRadius = 64.0f;

        // Use this for initialization
        void Start()
        {
            Morphology n1 = Morphology.test1();

            Morphology n2 = Morphology.test1();
            Morphology n3 = Morphology.test1();

            Morphology n4 = Morphology.test1();
            Morphology n5 = Morphology.test1();
            Morphology n6 = Morphology.test1();

            Morphology n7 = Morphology.test1();
            Morphology n8 = Morphology.test1();
            Morphology n9 = Morphology.test1();

            addChildNodeAtMorphology(null, n1);

            addChildNodeAtMorphology(n1, n2);
            addChildNodeAtMorphology(n1, n3);

            addChildNodeAtMorphology(n2, n4);
            addChildNodeAtMorphology(n2, n5);
            addChildNodeAtMorphology(n2, n6);

            addChildNodeAtMorphology(n3, n7);
            addChildNodeAtMorphology(n3, n8);
            addChildNodeAtMorphology(n3, n9);

            updateNodePositions();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void addChildNodeAtMorphology(Morphology parent, Morphology child)
        {
            if (child != null)
            {
                if (root != null)
                {
                    if (parent != null)
                    {
                        // Look for the node with the parent morphology
                        TreeNodeUI treeNode = recursiveFindNode(parent, root);

                        // Add the child to the node
                        if (treeNode != null)
                        {
                            treeNode.addChildNode(TreeNodeUI.createTreeNodeUI(child, this));
                        }
                    }
                }
                else
                {
                    root = TreeNodeUI.createTreeNodeUI(child, this);
                }
            }
        }


        private TreeNodeUI recursiveFindNode(Morphology src, TreeNodeUI node)
        {
            // Check if the node has this morphology 
            if (node.isMorphology(src))
            {
                return node;
            }

            // Keep finding the node with the wanted morphology recursively 
            foreach (TreeNodeUI n in node.getChildNodes())
            {
                TreeNodeUI foundNode = recursiveFindNode(src, n);

                // If we end looking in this branch check if we found something
                if (foundNode != null)
                    return foundNode;
            }

            // Return that nothing was found
            return null;
        }

        private void updateNodePositions()
        {
            if (root != null)
            {
                List<List<TreeNodeUI>> treeNodesByLayersList = new List<List<TreeNodeUI>>();

                getLayersListPositions(root, 0, treeNodesByLayersList);

                Vector2 position = Vector2.zero;
                float initialXPosition = 0.0f;
                float initialYPosition = 0.0f;
                int i = 0;
                Debug.Log(treeNodesByLayersList.Count);
                foreach (List<TreeNodeUI> nodeList in treeNodesByLayersList)
                {
                    // Update the first node X position for the next branch to the right
                    initialXPosition = -(nodeList.Count - 1) * 2 * nodeRadius - (nodeList.Count - 1) * nodeXSpacing;
                    initialXPosition /= 2;

                    // Start the count again
                    i = 0;

                    // Update position for each node in the layer
                    foreach (TreeNodeUI treeNode in nodeList)
                    {
                        // Update node position with the desired radius and spacing
                        treeNode.setPosition(initialXPosition + i * (2 * nodeRadius + nodeXSpacing), initialYPosition);
                        Debug.Log("YPosition: " + initialYPosition);
                        i++;
                    }
    
                    // Update the Y position for the next deeper Level
                    initialYPosition += nodeRadius * 2 + nodeYSpacing;
                }


                this.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, 0, 0);
            }
        }


        private void getLayersListPositions(TreeNodeUI node, int layer, List<List<TreeNodeUI>> list)
        {
            if (node != null)
            {
                // Add a new list at the layer in the list 
                if (layer + 1 > list.Count)
                    list.Add(new List<TreeNodeUI>());

                // Add Node to the layer list
                list[layer].Add(node);

                foreach (TreeNodeUI n in node.getChildNodes())
                {
                    // Go deep one level
                    layer++;

                    // Keep going deeper and deeper recursively
                    getLayersListPositions(n, layer, list);

                    // Return one level up
                    layer--;
                }
            }
        }
    }
}
