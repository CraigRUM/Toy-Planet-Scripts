﻿using UnityEngine;
using System.Collections;

public class Terrain : MonoBehaviour {

    /*
    ////////////////////
    state variables
    ////////////////////
    */
    public Material barron, empty, low, high;

    public Transform soildTile, liquidTile;
    public Transform raspBush, appTree, flower;
    public Transform cSpawner, hSpawner, oSpawner;

    public bool isTree = true, hasSeed = false, isWater = false, hasNest = false;

    SunControls sol;

    [Range(0, 2)]
    int SpawnerType;

    [Range(0,2)]
    public int ProducerType;

    bool hasTree = false;

    Transform currentTile;
    enum Type {Solid, Liquid}
    string[] tagList = { "Ground", "Water" };
    enum Abundance {Barron, Empty, Low, High}
    enum Height {Low, Mid, High }
    
    Type currentType;
    Height currentHight = Height.Mid;
    Abundance abundanceLevel;

    /*
    ////////////////////
    Initilization
    ////////////////////
    */

    void Start () {
        sol = FindObjectOfType<SunControls>();
        if (isWater == true){
            currentType = Type.Liquid;
            abundanceLevel = Abundance.Barron;
            InstanceType();
        }
        else {
            currentType = Type.Solid;
            abundanceLevel = Abundance.Empty;
            if (sol != null) { sol.Photosynthesis += UpdateAbundance; }
            InstanceType();
            setSurfaceTexture();
        }
        
    }




    /*
    ////////////////////
    Public Interactions
    ////////////////////
    */
    public void AddNest(int nestType) {
        hasNest = true;
        switch (nestType) {
            case 1:
                PlaceModel(0.5f,oSpawner,0);
                break;
            case 2:
                PlaceModel(0.5f, cSpawner, 0);
                break;
            default:
                PlaceModel(0.5f, hSpawner, 0);
                break;
        }
    }

    public bool HasReasource() {
        if (abundanceLevel != Abundance.Barron && currentType != Type.Liquid) { return true; }
        return false;
    }

    public bool Drink() {
        if (currentType == Type.Liquid) { return true; }
        return false;
    }

    public void Graze() {
        if (abundanceLevel != Abundance.Barron && currentType == Type.Solid) {
            abundanceLevel--;
            if (abundanceLevel == Abundance.Low && sol != null) {
                sol.Photosynthesis -= UpdateAbundance;
                sol.Photosynthesis += UpdateAbundance;
            }
            setSurfaceTexture();
        }
        }

    public void ToggleType() {
        if (currentType == Type.Solid) {
            currentType = Type.Liquid;
        } else {
            currentType = Type.Solid;
        }
        InstanceType();
    }

    public void Fertilize()
    {
        if (hasTree != true)
        { 
            if (abundanceLevel == Abundance.Barron)
            {
                abundanceLevel++;
                UpdateAbundance();
            }
        }
    }

    public void Fertilize(bool HasSeed, bool IsTree)
    {
        if (hasTree != true && currentType == Type.Solid)
        {
            hasSeed = HasSeed;
            isTree = IsTree;
            if (abundanceLevel == Abundance.Barron)
            {
                abundanceLevel++;
                UpdateAbundance();
            }
        }
    }

    public void OccuppyToggle(){
        if (hasNest == false){
            hasNest = true;}
        else { hasNest = false; }
    }

    public void SetTerrainHeight(float heightMod){
        if (heightMod > 0) { currentHight = Height.High; } else { currentHight = Height.Low; }
        Vector3 heightVector = new Vector3(0,heightMod,0);
        transform.position += heightVector;
        InstanceType();
    }

    public char heightCheck(){
        if (currentHight == Height.Low)
        {
            return 'l';
        }
        else if (currentHight == Height.High)
        {
            return 'h';
        }
        else
        {
            return 'm';
        }
    }
/*
////////////////////
Internal block updaters
////////////////////
*/

    void setSurfaceTexture()
    {
        if (transform.FindChild("TerrainTile(S)(Clone)"))
        {
            switch (abundanceLevel)
            {
                case Abundance.High:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = high;
                    if (sol != null && hasSeed != true) { sol.Photosynthesis -= UpdateAbundance; }
                    break;
                case Abundance.Low:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = low;
                    break;
                case Abundance.Empty:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = empty;
                    break;
                default:
                    transform.FindChild("TerrainTile(S)(Clone)").transform.FindChild("surface").gameObject.GetComponent<Renderer>().material = barron;
                    if(sol != null) { sol.Photosynthesis -= UpdateAbundance; }
                    
                    break;

            }
        }
    }

    void InstanceType()
    {
        if (hasNest == true) { currentType = Type.Solid; }
        if (transform.FindChild("TerrainTile(L)(Clone)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(L)(Clone)").gameObject);
        }
        if (transform.FindChild("TerrainTile(S)(Clone)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(S)(Clone)").gameObject);
        }
        if (transform.FindChild("TerrainTile(L)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(L)").gameObject);
        }
        if (transform.FindChild("TerrainTile(S)"))
        {
            DestroyImmediate(transform.FindChild("TerrainTile(S)").gameObject);
        }

        if (currentType == Type.Solid || hasNest == true)
        {
            currentTile = soildTile;
            Transform newTile = Instantiate(soildTile, transform.position, Quaternion.Euler(Vector3.right * 90)) as Transform;
            newTile.parent = transform;
            transform.tag = tagList[0];
            setSurfaceTexture();
        }
        else
        {
            Transform newTile = Instantiate(liquidTile, transform.position, Quaternion.Euler(Vector3.right * 90)) as Transform;
            newTile.parent = transform;
            transform.tag = tagList[1];
        }
    }

    void Grow()
    {
        if (hasSeed == true && abundanceLevel == Abundance.High)
        {
            GrowPrimaryProducer();
            setSurfaceTexture();
        }
        if (abundanceLevel != Abundance.High) {
            abundanceLevel++;
            setSurfaceTexture();
        }

    }

    void GrowPrimaryProducer()
    {
        if (currentType == Type.Solid && hasNest != true) {
            switch (ProducerType)
            {
                case 1:
                    PlaceModel(.5f, raspBush, 0);
                    hasTree = true;
                    break;
                case 2:
                    PlaceModel(1, appTree, 0); 
                    hasTree = true;
                    break;
                default:
                    PlaceModel(.65f, flower, 0);
                    break;
            }
            abundanceLevel = Abundance.Barron;
            setSurfaceTexture();
        }
    }

    void PlaceModel(float yOffSet, Transform modelType, int rotation)
    {
        Vector3 vecOffSet = new Vector3(0, yOffSet, 0);
        Transform updatedModel = Instantiate(modelType, transform.position + vecOffSet, Quaternion.Euler(Vector3.right * rotation)) as Transform;
        updatedModel.parent = transform;
    }

    /*
    ////////////////////
    Replament update Coroutine
    ////////////////////
    */

    void UpdateAbundance()
    {
        //State Check
        if (currentType == Type.Solid) { 
            if (abundanceLevel != Abundance.High || hasSeed == true && hasTree != true && hasNest != true)
            {
            //Debug.Log("sol has Granted me his strength!!!");
                Grow();
            }
        }
    }
}