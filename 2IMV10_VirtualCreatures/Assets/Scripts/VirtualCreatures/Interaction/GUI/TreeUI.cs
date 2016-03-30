using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VirtualCreatures
{
    public class TreeUI : MonoBehaviour
    {
        public GameObject nodesGO;
        public GameObject linesGO;

        public TreeNodeUI root;
        public float nodeXSpacing = 15.0f;
        public float nodeYSpacing = 15.0f;
        public float nodeRadius = 50.0f;

        private IList<UILineRenderer> lines = new List<UILineRenderer>();

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

            Morphology n10 = Morphology.test1();
            Morphology n11 = Morphology.test1();
            Morphology n12 = Morphology.test1();
            addChildNodeAtMorphology(null, n1);

            addChildNodeAtMorphology(n1, n2);
            addChildNodeAtMorphology(n1, n3);

            addChildNodeAtMorphology(n2, n4);
            addChildNodeAtMorphology(n2, n5);

            addChildNodeAtMorphology(n3, n6);
            addChildNodeAtMorphology(n3, n7);

            addChildNodeAtMorphology(n4, n8);
            addChildNodeAtMorphology(n5, n9);
            addChildNodeAtMorphology(n6, n10);

            addChildNodeAtMorphology(n10, n11);
            addChildNodeAtMorphology(n10, n12);

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
                            if (nodesGO != null)
                            {
                                treeNode.addChildNode(TreeNodeUI.createTreeNodeUI(child, nodesGO));
                            }
                        }
                    }
                }
                else
                {
                    if (nodesGO != null)
                    {
                        root = TreeNodeUI.createTreeNodeUI(child, nodesGO);
                    }
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

                getLayersListPositions(treeNodesByLayersList);

                // Initialize variables
                Vector2 position = Vector2.zero;
                float initialXPosition = 0.0f;
                float initialYPosition = 0.0f;
                int i = 0;

                // Consider UI Object scale for positions
                float scaleX = this.transform.parent.transform.lossyScale.x;
                float scaleY = this.transform.parent.transform.lossyScale.y;

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
                        treeNode.setPosition(scaleX * (initialXPosition + i * (2 * nodeRadius + nodeXSpacing)), scaleY * initialYPosition);
                        i++;
                    }
    
                    // Update the Y position for the next deeper Level
                    initialYPosition += nodeRadius * 2 + nodeYSpacing;
                }

                // 
                updateLines();

                // Position in the middle
                this.GetComponent<RectTransform>().position = new Vector3(Screen.width / 2, scaleY * (nodeRadius * 2 + nodeYSpacing), 0);
            }
        }

        private void updateLines()
        {
            // Destroy the lines to update them
            // Very inefficient but if we want to remove nodes?
            if (lines.Count != 0)
            {
                foreach (UILineRenderer line in lines)
                {
                    Destroy(line);
                    lines.Clear();
                }
            }

            updateLines(root);
        }

        private void updateLines(TreeNodeUI node)
        {
            if(node != null && linesGO !=null) 
            {
                foreach (TreeNodeUI n in node.getChildNodes())
                {
                    // Instantiate a line object
                    GameObject lineGO = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GUI/LineUI.prefab"));

                    lineGO.transform.SetParent(linesGO.transform);
                    // Get the line component and edit position
                    UILineRenderer line = lineGO.GetComponent<UILineRenderer>();
                    line.Points[0] = new Vector2(node.position.x, node.position.y) ;
                    line.Points[1] = new Vector2(n.position.x, n.position.y);


                    // Add the line to the list
                    lines.Add(line);

                    // Keep going deeper and deeper recursively
                    updateLines(n);
                }
            }
        }

        private void getLayersListPositions(List<List<TreeNodeUI>> list)
        {
            getLayersListPositions(root, 0, list);
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
