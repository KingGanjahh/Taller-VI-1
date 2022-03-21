using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.Tilemaps;

namespace PathFinding
{
    public class PathMovement : MonoBehaviour
    {
        [SerializeField] private Pathfinding2D pathMovement;
        [SerializeField] private Unit selectedUnit;
        [SerializeField] private GameObject target;
        [SerializeField] private Tilemap map;
        private bool grabed;
        private bool selectedNewSpace;

        [SerializeField] private TileBase tile;

        //From here, TurnSystem

        [SerializeField] private TurnSystem.TurnSystem turnSystem;
        
        //From here, SoundSystem

        [SerializeField] private AudioManager source;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !grabed && !selectedNewSpace)
            {
                SelectUnit(); 
            }
            else if (Input.GetMouseButtonDown(0) && grabed && !selectedNewSpace)
            {
                SelectNewSpace();
            }
        }

        private void SelectUnit()
        {
            Vector2 worldPosition = turnSystem.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitData = Physics2D.Raycast(worldPosition, Vector2.zero, 0);

            if (!hitData)
            {
                grabed = false;
                return;
            }
            if (hitData.transform.gameObject.CompareTag("Enemy"))
            {
                grabed = false;
            }
            if (hitData.transform.gameObject.CompareTag("Ally"))
            {
                source.Play("SelectedUnit");
                
                selectedUnit = hitData.transform.gameObject.GetComponent<Unit>();
                pathMovement = selectedUnit.GetComponent<Pathfinding2D>();
                
                selectedUnit.path.SetActive(true);
                
                grabed = true;
                
                //turnSystem.mainCamera.transform.DOMove(new Vector3(0, 0, -10) + selectedUnit.transform.position, 0.2f, false);
                selectedUnit.anim.SetBool("Walk2", true);

                if (selectedUnit.hasMoved)
                {
                    grabed = false;
                    selectedUnit.path.SetActive(false);
                    
                    selectedUnit.anim.SetBool("Walk2", false);
                }
            }
        }

        void SelectNewSpace()
        {
            if (grabed)
            {
                Vector2 worldPosition = turnSystem.mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hitData = Physics2D.Raycast(worldPosition, Vector2.zero, 0);
                
                if (!hitData)
                {
                    source.Play("SelectedSpace");
                
                    Vector2 mousePosition = turnSystem.mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 gridPosition = map.WorldToCell(mousePosition);

                    var newTarget = Instantiate(target, gridPosition, quaternion.identity);
                    selectedNewSpace = true;

                    Vector3Int tilePosition = map.WorldToCell(mousePosition);
                    if (map.GetTile(tilePosition) == null)
                    {
                        grabed = false;
                        selectedNewSpace = false;
                        selectedUnit.path.SetActive(false);
                        selectedUnit.anim.SetBool("Walk2", false);
                        Destroy(newTarget);
                        return;
                    }

                    Vector3Int unitGridPos = map.WorldToCell(selectedUnit.transform.position);
                    Vector3Int targetGridPos = map.WorldToCell(newTarget.transform.position);

                    pathMovement.FindPath(unitGridPos, targetGridPos);

                    if (pathMovement.path.Count > selectedUnit.movement)
                    {
                        grabed = false;
                        selectedNewSpace = false;
                        selectedUnit.path.SetActive(false);
                        selectedUnit.anim.SetBool("Walk2", false);
                        Destroy(newTarget);
                        return;
                    }
            
                    Move(pathMovement);

                    grabed = false;
                    selectedNewSpace = false;
                    Destroy(newTarget);
                }
                else
                {
                    grabed = false;
                    selectedUnit.path.SetActive(false);
                    
                    selectedUnit.anim.SetBool("Walk2", false);
                }
            }
            else
            {
                grabed = false;
            }
        }

        private void Move(Pathfinding2D unitPath)
        {
            selectedUnit.path.SetActive(false);
            
            foreach (var t in unitPath.path)
            { 
                selectedUnit.transform.DOMove(t.worldPosition, 0.5f, true);
            }

            selectedUnit.anim.SetBool("Walk2", false);
             
            selectedUnit.hasMoved = true;
        }
    }
}
