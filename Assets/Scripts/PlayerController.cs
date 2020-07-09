using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private TerrainManager terrainManager;

        [SerializeField]
        private Transform playerHead;

        [SerializeField]
        private TerrainConfig config;

        private Collider playerCollider;

        private readonly Stack<TerrainBlock.Type> blocksInventory = new Stack<TerrainBlock.Type>();

        private void Start()
        {
            playerCollider = GetComponentInChildren<Collider>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsSimulationKeyPressed())
                {
                    terrainManager.MakeSimulatedBlock(GetViewRay(), true);
                }
                else
                {
                    RemoveBlockAndAddToInventory(GetViewRay());
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (blocksInventory.Count != 0)
                {
                    var block = terrainManager.AddBlock(GetViewRay(), blocksInventory.Pop());

                    if (block != null)
                    {
                        if (block.GetBounds().Intersects(playerCollider.bounds))
                        {
                            transform.position += Vector3.up * config.BlockSize;
                        }

                        if (IsSimulationKeyPressed())
                        {
                            terrainManager.MakeSimulatedBlock(GetViewRay(), false);
                        }
                    }
                }
            }
        }

        private static bool IsSimulationKeyPressed()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private void RemoveBlockAndAddToInventory(Ray viewRay)
        {
            var removedBlockType = terrainManager.RemoveBlock(viewRay);

            if (removedBlockType.HasValue)
            {
                blocksInventory.Push(removedBlockType.Value);
            }
        }

        private Ray GetViewRay()
        {
            return new Ray(playerHead.position, playerHead.forward);
        }
    }
}