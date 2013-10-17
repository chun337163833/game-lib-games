using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using Engine.Data.Json;
using Engine.Events;
using Engine.Utility;

public class GameObjectChoiceMessages {
    public static string gameChoiceDataResponse = "game-choice-data-response";
}

public class GameObjectChoiceData {
    public string choiceCode = "";
    public string choiceType = "";
    public string choiceItemCode = "";
    public string choiceItemDisplay = "";
    public string choiceItemAssetCode = "";
    public bool choiceItemIsCorrect = true;
}

public class GameObjectChoice : GameObjectLevelBase {

    public AppContentChoice appContentChoice;
    public AppContentChoiceItem appContentChoiceItem;

    public GameObjectChoiceData choiceData;

    public GameObject containerLabel;
    public UILabel labelResponse;

    public GameObject containerEffects;
    public GameObject containerEffectsAlwaysOn;
    public GameObject containerEffectsCorrect;
    public GameObject containerEffectsIncorrect;

    public GameObject containerAsset;

    public Color startColor = Color.red;

    public bool isUI = false;

    public bool hasBroadcasted = false;

    public override void Start() {
        base.Start();

        LoadData();
    }

    public override void LoadData() {
        base.LoadData();

        LoadChoice("question-1", "correct", true, code, "false","barrel-1");

        if(containerEffectsCorrect != null) {
            containerEffectsCorrect.StopParticleSystem(true);
        }
        if(containerEffectsIncorrect != null) {
            containerEffectsIncorrect.StopParticleSystem(true);
        }

        SetChoiceParticleSystemColors();

        if(isUI) {
            gameObject.SetLayerRecursively("UIOverlay");
        }
    }

    public void SetChoiceParticleSystemColors() {
        if(containerEffectsAlwaysOn != null) {
            containerEffectsAlwaysOn.SetParticleSystemStartColor(startColor, true);
        }
    }

    public void LoadAsset(string assetCode) {

        if(containerAsset != null) {

            containerAsset.DestroyChildren();

            GameObject assetItem = GameObjectHelper.LoadFromResources(
                Path.Combine(
                Contents.appCacheVersionSharedPrefabLevelAssets,
                assetCode));

            if(assetItem != null) {
                if(isUI) {
                    assetItem.SetLayerRecursively("UIOverlay");
                }
                else {
                    assetItem.SetLayerRecursively("Default");
                }
                assetItem.transform.parent = containerAsset.transform;
                //assetItem.transform.position = containerAsset.transform.position;
                //assetItem.transform.rotation = containerAsset.transform.rotation;
                assetItem.transform.localPosition = Vector3.zero;
                assetItem.transform.localRotation = Quaternion.identity;
                assetItem.transform.localRotation = Quaternion.identity;
                assetItem.transform.localScale = Vector3.one;

                foreach(Rigidbody rigidbody in assetItem.GetComponentsInChildren<Rigidbody>(true)) {

                    GameObject go = rigidbody.gameObject;

                    go.AddComponent<GameObjectChoiceAsset>();
                    go.ResetPosition(true);
                    go.ResetRotation(true);
                }

                //assetItem.ResetPosition();
                //assetItem.ResetRotation();
            }
        }
    }

    public void LoadChoiceItem(AppContentChoice choice, AppContentChoiceItem choiceItem) {
        appContentChoice = choice;
        appContentChoiceItem = choiceItem;

        LoadChoice(
            appContentChoice.code,
            appContentChoice.type,
            appContentChoiceItem.IsTypeCorrect(),
            appContentChoiceItem.display,
            appContentChoiceItem.code, "barrel-1");
    }


    public void LoadChoice(
        string choiceCode,
        string choiceType,
        bool choiceItemIsCorrect,
        string choiceItemDisplay,
        string choiceItemCode,
        string choiceItemAssetCode) {

        choiceData = new GameObjectChoiceData();
        choiceData.choiceCode = choiceCode;
        choiceData.choiceType = choiceType;
        choiceData.choiceItemIsCorrect = choiceItemIsCorrect;
        choiceData.choiceItemDisplay = choiceItemDisplay;
        choiceData.choiceItemCode = choiceItemCode;
        choiceData.choiceItemAssetCode = choiceItemAssetCode;

        if(appContentChoice == null) {
            appContentChoice = AppContentChoices.Instance.GetByCode(choiceCode);

            foreach(AppContentChoiceItem choiceItem in appContentChoice.choices) {
                if(choiceItem.code == choiceItemCode) {
                    appContentChoiceItem = choiceItem;
                }
            }
        }

        LoadAsset(choiceItemAssetCode);

        //Debug.Log("LoadChoice:SetLabel:choiceData.choiceItemDisplay:" + choiceData.choiceItemDisplay);

        UIUtil.SetLabelValue(labelResponse, choiceData.choiceItemDisplay);
        //Debug.Log("LoadChoice:SetLabel:labelResponse:" + labelResponse.text);
    }

    public void BroadcastChoice() {

        if(!hasBroadcasted) {

            hasBroadcasted = true;

            Debug.Log("GameObjectChoice:BroadcastChoice:" + appContentChoiceItem.code);

            Messenger<GameObjectChoiceData>.Broadcast(
                GameObjectChoiceMessages.gameChoiceDataResponse, choiceData);

            //Messenger<AppContentChoiceItem>.Broadcast(
            //    AppContentChoiceMessages.appContentChoiceItem, appContentChoiceItem);

       // Messenger<AppContentChoiceItem>.RemoveListener(AppContentChoiceMessages.appContentChoiceItem, OnAppContentChoiceItemHandler);
        }
    }

    public void BroadcastChoiceDelayed(float delay) {
        if(!hasBroadcasted) {
            StartCoroutine(BroadcastChoiceDelayedCo(delay));
        }
    }

    public IEnumerator BroadcastChoiceDelayedCo(float delay) {
        yield return new WaitForSeconds(delay);
        BroadcastChoice();
    }

    public void HandleChoiceData() {

        Debug.Log("GameObjectChoice:HandleChoiceData:" + name);

        if(choiceData != null) {

            if(!choiceData.choiceItemIsCorrect) {

                // Play correct
                if(containerEffectsCorrect != null) {

                    if(containerEffectsCorrect != null) {
                        containerEffectsCorrect.PlayParticleSystem(true);
                    }

                    BroadcastChoiceDelayed(2f);
                }

            }
            else {

                // Play incorrect

                if(containerEffectsIncorrect != null) {
                    containerEffectsIncorrect.PlayParticleSystem(true);
                }

                BroadcastChoiceDelayed(2f);

                if(gamePlayerController != null) {
                    gamePlayerController.AddImpact(Vector3.back, 1f);
                }

                if(rigidbody != null) {
                    rigidbody.AddExplosionForce(100f, transform.position, 50f);
                }
            }
        }
    }

    GamePlayerController gamePlayerController = null;

    public void HandleCollision(Collision collision) {

        // If the human player hit us, check the score/choice and correct or incorrect message broadcast

        Debug.Log("GameObjectChoice:OnCollisionEnter:" + collision.transform.name);

        GameObject go = collision.collider.transform.gameObject;

        Debug.Log("GameObjectChoice:go:" + go.name);

        if(go.name.Contains("GamePlayerObject")) {

            gamePlayerController = GameController.GetGamePlayerController(go);

            if(gamePlayerController != null) {

                if(gamePlayerController.IsPlayerControlled) {

                    HandleChoiceData();
                }
            }
        }

        if(gamePlayerController == null
            && (go.name.Contains("Helmet")
            || go.name.Contains("Facemask"))) {

            Debug.Log("GameObjectChoice:HelmetFacemask:" + go.name);

            gamePlayerController = GameController.GetGamePlayerControllerParent(go);

            if(gamePlayerController != null) {

                Debug.Log("GameObjectChoice:gamePlayerController:" + gamePlayerController.name);

                if(gamePlayerController.IsPlayerControlled) {

                    HandleChoiceData();
                }
            }
        }
    }

    public void OnCollisionEnter(Collision collision) {

        HandleCollision(collision);
    }
}
