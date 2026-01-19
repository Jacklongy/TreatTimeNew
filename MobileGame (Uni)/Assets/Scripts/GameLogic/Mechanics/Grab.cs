using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grabbing items on the Grid and from the inventory, Also manages Merging. 
/// </summary>
public class Grab : MonoBehaviour
{
    // Dragging
    [HideInInspector]
    public bool Isdragging;
    public GameObject currentlyDragging;

    // Ray casts
    [HideInInspector]
    public GameObject HitObject;
    public GameObject TileHit;
    public GameObject LastTileHit;

    // Dog Animator
    [Header("Dog Animation")]
    public Animator dog;

    [Header("Merge Detection")]
    public float mergeDetectionRadius = 0.5f; // Radius for detecting nearby items to merge

    // The Position where we tap!
    Vector3 worldPos;

    public bool NoPickUp;


    private void Start()
    {
        NoPickUp = false;
    }

    private void Update()
    {
        #region Touch Controls
        // so if we touch the screen. 
        if (Input.touchCount > 0)
        {
            // find out where that touch is... 
            // this line get where you first touch the screen. Input.GetTouch(1) would be the second touch. 
            Touch touch = Input.GetTouch(0);

            // this is the position of where you touched ON SCREEN
            Vector2 touchPos = touch.position;

            // So convert it to world spacee

            // we choose a vector3 here to manage the depth which will litterally be at the near clip plane, then we create a new Vector3 to store the x, y of where we touched. 
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, Camera.main.nearClipPlane));

            // we now use worldPos as our reference of where we touched on screen.

            // Different touch phases.
            switch (touch.phase)
            {
                // began is when you first touch the screen!
                case TouchPhase.Began:

                    Begin();

                    break;

                // moved is while the finger is moving
                case TouchPhase.Moved:

                    Dragging();

                    break;

                // ended is when you let go
                case TouchPhase.Ended:

                    End();

                    break;

                // if finger is dragged of screen.
                case TouchPhase.Canceled:

                    Cancel();

                    break;

                default:
                    break;
            }

        }
        else
        {
            HandleMouseInput();
        }
        #endregion  
    }


    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse position and convert to world space
            Vector3 mousePos = Input.mousePosition;
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

            Begin();
        }
        else if (Input.GetMouseButton(0))
        {
            // Update world position while dragging
            Vector3 mousePos = Input.mousePosition;
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

            Dragging();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            End();
        }

    }

    public void Begin()
    {
        #region First Raycast 

        if(NoPickUp == false)
        {
            // make sure we only hit these layers. 
            int Layers = LayerMask.GetMask("Dropped", "InInv");

            // we send a ray from where we touch on screen, straight ahead for infinate distance.
            RaycastHit2D hit2D = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, Layers);

            // if we hit a collider, and that colided item inherits Item class... 
            if (hit2D.collider != null && hit2D.collider.gameObject.GetComponent<Item>())
            {
                //check What item we hit
                Debug.LogWarning(hit2D.collider.name);

                // we set the item we just hit to the item we are now dragging.
                currentlyDragging = hit2D.collider.gameObject;

                // we are now dragging an item.
                Isdragging = true;

                // Get the item class
                Item item = currentlyDragging.GetComponent<Item>();

                // Item class is on the item. 
                if (item != null)
                {
                    // call grabbed on that item.
                    item.Grabbed();
                }

                currentlyDragging.layer = 2;
            }
            #endregion

            #region Remove from the Tile
            // if we are currently dragging an object...
            if (currentlyDragging != null)
            {
                // Send another ray cast after pickup to remove the item from that current grid tile.
                RaycastHit2D GridCheck = Physics2D.Raycast(currentlyDragging.transform.position, Vector3.forward);

                // If the grid slot has been colided with
                if (GridCheck.collider != null)
                {
                    // we get the reference to the tile script on the tile.
                    TileHit = GridCheck.collider.gameObject;

                    if (TileHit.GetComponent<Tile>())
                    {
                        // and we remove the object from the tile. 
                        TileHit.GetComponent<Tile>().RemoveObject();

                        LastTileHit = TileHit;

                        TileHit = null;
                    }
                }
            }
            #endregion
        }

    }
    public void Dragging()
    {
        #region Dragging
        // so if is dragging is true and we have a game object...
        if (Isdragging == true && currentlyDragging != null)
        {

            if(currentlyDragging.GetComponent<Bowl>() != null && currentlyDragging.GetComponent<Bowl>().ItemInBowl != null)
            {
                dog.SetBool("Hovering", true);

                // close eyes here aswell or something
            }

            Item item = currentlyDragging.GetComponent<Item>();

            SpriteRenderer spriteRenderer = currentlyDragging.GetComponent<SpriteRenderer>();

            spriteRenderer.sortingLayerName = "Dragging";

            foreach (Transform child in currentlyDragging.transform)
            {
                SpriteRenderer spriteRendererChild = child.GetComponent<SpriteRenderer>();

                     spriteRendererChild.sortingLayerName = "Dragging";

                if (child.childCount > 0)
                {
                    foreach (Transform childChild in child.transform)
                    {
                        SpriteRenderer spriteRendererChildChild = childChild.GetComponent<SpriteRenderer>();

                        spriteRendererChildChild.sortingLayerName = "Dragging";
                    }
                }

            }

            if (item.OnGrid == true)
            {
                // we basically set the objects position to where our finger is. When item is on grid. 
                currentlyDragging.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z);
            }
            else
            {
                // All items are on grid now - no offset needed
                currentlyDragging.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z);
            }
        }
        #endregion
    }

    
    public void End()
    {

        if (currentlyDragging != null)
        {

            dog.SetBool("Hovering", false);

            // Change Sorting Layer Back just to foreground and not dragging.
            SpriteRenderer spriteRenderer = currentlyDragging.GetComponent<SpriteRenderer>();

            spriteRenderer.sortingLayerName = "foreGround";


            foreach (Transform child in currentlyDragging.transform)
            {
               
                SpriteRenderer spriteRendererChild = child.GetComponent<SpriteRenderer>();

                spriteRendererChild.sortingLayerName = "foreGround";

                if(child.childCount > 0)
                {
                    foreach (Transform childChild in child.transform)
                    {
                        SpriteRenderer spriteRendererChildChild = childChild.GetComponent<SpriteRenderer>();

                        spriteRendererChildChild.sortingLayerName = "foreGround";
                    }
                }

            }



            // We send a new ray cast when we let go of the object to see if we are above something we can merge with...
            // Use OverlapCircle to detect nearby items within merge radius
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(currentlyDragging.transform.position, mergeDetectionRadius);

            Collider2D hitCollider = null;
            
            // Find the first valid collider to interact with (not the item itself)
            foreach (Collider2D col in hitColliders)
            {
                if (col.gameObject != currentlyDragging)
                {
                    hitCollider = col;
                    break;
                }
            }

    
            // so if we hit something underneath our currently dragging object...
            if (hitCollider != null)
            {
                // reference to that hit object.  this isnt nessasary but streamlines code a little better.
                HitObject = hitCollider.gameObject;

                // so here what do we want to do.. we want to check what we have hit. (Talking to self via code comments im going mad lol)

                // We can seperate the logic for the bowls and The food for more read ability. 
                // do this for last min clean ups. For now focusing on main features. 

                // If we hit the dog when we drop, and the item is not a bowl with food. then snap back to a tile if it was already on the grid. 
                if(hitCollider.gameObject.CompareTag("Dog") && currentlyDragging.GetComponent<Bowl>() != null && 
                    currentlyDragging.GetComponent<Bowl>().ItemInBowl == null && currentlyDragging.GetComponent<Item>().GridCheck())
                {
                    SnapBackToTile();
                }
                // else if the item was taken from the grid. snap back to the tile. 
                else if (hitCollider.gameObject.CompareTag("Dog") && currentlyDragging.GetComponent<Bowl>() != null &&
                    currentlyDragging.GetComponent<Bowl>().ItemInBowl == null && currentlyDragging.GetComponent<Item>().GridCheck() == false)
                {
                    // All items on grid now - snap back to tile
                    SnapBackToTile();
                }


                #region Bowl Checks and Feeding dog
                // If the item we are dragging can be put in a bowl, and we hit a bowl, add that item to the bowl
                else if (hitCollider != null && currentlyDragging.GetComponent<IBowlable>() != null && hitCollider.gameObject.GetComponent<IBowl>() != null &&
                    hitCollider.gameObject.GetComponent<IBowl>().CheckFull() != true)
                {
                    IBowl bowl = hitCollider.gameObject.GetComponent<IBowl>();

                    bowl.TakeItem(currentlyDragging);

                    Clear();
                }
               
              
                // If the Item we are currently dragging can feed the dog, and we hit the dog feed the dog. 
                else if (currentlyDragging.GetComponent<IFeedDog>() != null && hitCollider.gameObject.GetComponent<DogFeeder>() != null)
                {
                    IFeedDog bowl = currentlyDragging.GetComponent<IFeedDog>();

                    Clear();

                    bowl.WhatsInBowl();

                    return;
                }

                else if(hitCollider.gameObject.GetComponent<IBowl>() != null && HitObject.GetComponent<Bowl>().ItemInBowl != null)
                {
                    // snap back to last grid spot instead.
                    SnapBackToTile();

                    return;
                }

                #endregion

                #region Merging
                // if it has the same tag as the item we are currently holding then merge. 
                else if (HitObject.CompareTag(currentlyDragging.tag) && HitObject.GetComponent<Item>() != null && currentlyDragging != HitObject)
                {
                    Item item = currentlyDragging.GetComponent<Item>();

                    item.MergeFromInvCheck();

                    Merge(HitObject);

                    Clear();
                }


                #endregion

                #region Tiles and Item Slots

                // if we hit a tile that isnt full. let that tile take the item. 
                else if (HitObject.GetComponent<Tile>() != null && HitObject.GetComponent<Tile>().IsFull == false
                    && HitObject.GetComponent<Tile>().CompareTag(currentlyDragging.GetComponent<Item>().GridTag()))
                {

                    HitObject.GetComponent<Tile>().TakeObject(currentlyDragging);

                    currentlyDragging.layer = 7;

                    Item item = currentlyDragging.GetComponent<Item>();

                    item.Dropped();

                    Clear();
                }

                
                #region snapping back
                else if (currentlyDragging.GetComponent<Item>().GridCheck())
                {
                    // snap back to last grid spot instead. 
                    SnapBackToTile();
                }
                else
                {
                    // All items are on grid now - snap back to last tile
                    SnapBackToTile();
                }

            }

            else if (currentlyDragging.GetComponent<Item>().GridCheck())
            {
                // snap back to last grid spot instead. 
                SnapBackToTile();
            }

            else
            {
                // All items are on grid - snap back to tile
                SnapBackToTile();
            }

           


            #endregion



            #endregion
        }
        else
        {
            Clear();
        }

    }

    #region Future clean ups. 
    public void Food(GameObject Hit)
    {

    }

    public void Bowl(GameObject Hit)
    {

    }
    #endregion

    /// <summary>
    /// Snap back to slot logic. 
    /// </summary>
    public void SnapBackToSlot()
    {
        FindObjectOfType<SoundManagerScript>().Play("CantPlace");

        Item Return = currentlyDragging.GetComponent<Item>();

        Return.ReturnToMySlot();

        currentlyDragging.layer = 3;

        Clear();
    }

    /// <summary>
    /// Snap to nearest empty tile logic. 
    /// </summary>
    public void SnapBackToTile()
    {
        GridManager gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            Clear();
            return;
        }

        // Get the nearest empty tile to current drop position
        Tile nearestTile = gridManager.GetNearestEmptyTile(currentlyDragging.transform.position);

        if (nearestTile != null)
        {
            // Place on nearest tile
            nearestTile.TakeObject(currentlyDragging);
            currentlyDragging.layer = 7;
            FindObjectOfType<SoundManagerScript>().Play("CantPlace");
        }
        else
        {
            // No empty tiles - snap back to original tile
            if (LastTileHit != null)
            {
                LastTileHit.GetComponent<Tile>().TakeObject(currentlyDragging);
                currentlyDragging.layer = 7;
                Debug.LogWarning("No empty tiles available, snapped back to original tile");
            }
        }

        Clear();
    }

    /// <summary>
    /// Cancelled grab, so if finger is dragged of screen etc.
    /// </summary>
    public void Cancel()
    {
        if (currentlyDragging != null)
        {
            SpriteRenderer spriteRenderer = currentlyDragging.GetComponent<SpriteRenderer>();

            spriteRenderer.sortingLayerName = "foreGround";

            Item Return = currentlyDragging.GetComponent<Item>();

            Return.ReturnToMySlot();

            currentlyDragging.layer = 7;

            Clear();
        }
    }

    /// <summary>
    /// Merge function. 
    /// </summary>
    /// <param name="New"> The item to merge </param>
    public void Merge(GameObject New)
    {
        Item merge = New.GetComponent<Item>();

        merge.Merge(New);

        Destroy(currentlyDragging);
    }


    // Clear makes sure when we drop something everything is reset correctly. 
    public void Clear()
    {
        Isdragging = false;
        currentlyDragging = null;
        HitObject = null;
        LastTileHit = null;
    }



    // old code
//         case TouchPhase.Began:

//                        // Ray cast 2D send a raycast from where we touch and in the direction of zero so striaght ahead!
//                        RaycastHit2D hit2D = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity);

//                    // if we hit an object with a collider and it has the grabablle commpontent...
//                    if (hit2D.collider != null && hit2D.collider.gameObject.GetComponent<Grabable>())
//                        {

//                        Debug.LogWarning(hit2D.collider.name);

//                        // we need a reference to the game object we have hit! 
//                        currentlyDragging = hit2D.collider.gameObject;

//                            Isdragging = true;


//                            // find merge interface for animations on grab
//                            IMerge merge = currentlyDragging.GetComponent<IMerge>();

//                        if(merge != null)
//                        {
//                            merge.Grabbed();
//                        }


//// set the current layer of the dragging object to ignore raycasts so it doesnt hit itself.
//currentlyDragging.layer = 2;

//// if we are currently dragging an object...
//if (currentlyDragging != null)
//{
//    // Send another ray cast after pickup to remove the item from that current grid tile.
//    RaycastHit2D GridCheck = Physics2D.Raycast(currentlyDragging.transform.position, Vector3.forward);

//    // If the grid slot has been colided with
//    if (GridCheck.collider != null)
//    {
//        // we get the reference to the tile script on the tile.
//        TileHit = GridCheck.collider.gameObject;

//        if (TileHit.GetComponent<Tile>())
//        {
//            // and we remove the object from the tile. 
//            TileHit.GetComponent<Tile>().RemoveObject();

//            LastTileHit = TileHit;

//            TileHit = null;
//        }
//    }
//}
//                        }
                    
                    
//                    break;

//                // moved is while the finger is moving
//                case TouchPhase.Moved:


//    // so if is dragging is true and we have a game object...
//    if (Isdragging == true && currentlyDragging != null)
//    {
//        // we basically set the objects position to where our finger is.
//        currentlyDragging.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z);
//    }

//    break;

//// ended is when you let go
//case TouchPhase.Ended:

//    if (currentlyDragging != null)
//    {

//        // We send a new ray cast when we let go of the object to see if we are above something we can merge with...
//        RaycastHit2D hitMerge = Physics2D.Raycast(currentlyDragging.transform.position, Vector3.forward);

//        Debug.DrawRay(currentlyDragging.transform.position, Vector3.forward, Color.red, 5f);

//        // so if we hit something
//        if (hitMerge.collider != null)
//        {
//            // reference to that hit object.
//            HitObject = hitMerge.collider.gameObject;

//            if (currentlyDragging.GetComponent<IBowlable>() != null && hitMerge.collider.gameObject.GetComponent<IBowl>() != null)
//            {
//                IBowl bowl = hitMerge.collider.gameObject.GetComponent<IBowl>();

//                bowl.TakeItem(currentlyDragging);

//                Isdragging = false;
//                currentlyDragging = null;
//                HitObject = null;
//                LastTileHit = null;

//                return;
//            }

//            else if (currentlyDragging.GetComponent<IFeedDog>() != null && hitMerge.collider.gameObject.GetComponent<DogFeeder>() != null)
//            {

//                IFeedDog bowl = currentlyDragging.GetComponent<IFeedDog>();

//                Isdragging = false;
//                currentlyDragging = null;
//                HitObject = null;
//                LastTileHit = null;

//                bowl.WhatsInBowl();

//                return;
//            }

//            // if it has the same tag as the item we are currently holding then merge. 
//            else if (HitObject.CompareTag(currentlyDragging.tag) && HitObject.layer == 7)
//            {
//                IMerge merge = currentlyDragging.GetComponent<IMerge>();

//                merge.MergeFromInvCheck();


//                Merge(HitObject);

//                Isdragging = false;
//                currentlyDragging = null;
//                HitObject = null;
//                LastTileHit = null;
//            }

//            // If we hit a tile and the tile is full
//            else if (HitObject.GetComponent<Tile>() != null && HitObject.GetComponent<Tile>().IsFull == false)
//            {

//                HitObject.GetComponent<Tile>().TakeObject(currentlyDragging);

//                currentlyDragging.layer = 7;

//                IMerge merge = currentlyDragging.GetComponent<IMerge>();
//                merge.Dropped();


//                Isdragging = false;
//                currentlyDragging = null;
//                HitObject = null;
//                LastTileHit = null;
//            }

//            else if (currentlyDragging.GetComponent<IMerge>().GridCheck())
//            {
//                // snap back to last grid spot instead. 

//                LastTileHit.GetComponent<Tile>().TakeObject(currentlyDragging);

//                currentlyDragging.layer = 7;
//                currentlyDragging = null;
//                Isdragging = false;
//                LastTileHit = null;

//            }
//            else
//            {
//                IMerge Return = currentlyDragging.GetComponent<IMerge>();

//                Return.ReturnToSlot();

//                currentlyDragging.layer = 3;
//                currentlyDragging = null;
//                Isdragging = false;
//                LastTileHit = null;

//                Debug.Log("Slot full");
//            }

//        }

//        else if (currentlyDragging.GetComponent<IMerge>().GridCheck())
//        {
//            // snap back to last grid spot instead. 
//            LastTileHit.GetComponent<Tile>().TakeObject(currentlyDragging);

//            currentlyDragging.layer = 7;
//            currentlyDragging = null;
//            Isdragging = false;
//            LastTileHit = null;

//        }

//        else
//        {
//            IMerge Return = currentlyDragging.GetComponent<IMerge>();

//            Return.ReturnToSlot();

//            currentlyDragging.layer = 3;
//            currentlyDragging = null;
//            Isdragging = false;
//            LastTileHit = null;


//            Debug.Log("Not hit");

//        }
//    }

//    break;


//// if finger is dragged of screen.
//case TouchPhase.Canceled:

//    if (currentlyDragging != null)
//    {

//        IMerge Return = currentlyDragging.GetComponent<IMerge>();

//        Return.ReturnToSlot();

//        currentlyDragging.layer = 7;
//        //currentlyDragging.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
//        Isdragging = false;
//        currentlyDragging = null;
//        LastTileHit = null;

//        // here we want to return the game object to an empty slot in the inventory.
//    }


//    break;

//default:
//    break;
//}

}
