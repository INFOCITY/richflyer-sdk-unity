package jp.co.infocity.richflyer;

import android.content.Context;
import android.os.Bundle;
import android.content.Intent;
import android.content.SharedPreferences;

import androidx.annotation.NonNull;

import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import jp.co.infocity.richflyer.action.RFAction;
import jp.co.infocity.richflyer.action.RFActionListener;

public class RFUnityPlayerActivity extends com.google.firebase.MessagingUnityPlayerActivity {
    private boolean unityBridgeReady;
    private Intent lastProcessedRichFlyerIntent;
    private Intent pendingRichFlyerIntent;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onResume() {
        super.onResume();

        queueRichFlyerActionEvent(getIntent());
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        queueRichFlyerActionEvent(intent);
    }

    public void setRichFlyerBridgeReady() {
        unityBridgeReady = true;
        sendPendingRichFlyerActionEvent();
    }

    private void queueRichFlyerActionEvent(Intent intent) {
        if (intent == null
                || intent == lastProcessedRichFlyerIntent
                || !RichFlyer.richFlyerAction(intent)) {
            return;
        }

        pendingRichFlyerIntent = intent;
        sendPendingRichFlyerActionEvent();
    }

    private void sendPendingRichFlyerActionEvent() {
        if (!unityBridgeReady || pendingRichFlyerIntent == null) {
            return;
        }

        SharedPreferences pref = getApplicationContext().getSharedPreferences("jp.co.infocity.richflyer.preferences", Context.MODE_PRIVATE);
        String callbackObject = pref.getString("RFContentCallbackTargetObject", "");
        if (callbackObject.isEmpty()) {
            return;
        }

        Intent targetIntent = pendingRichFlyerIntent;
        pendingRichFlyerIntent = null;
        lastProcessedRichFlyerIntent = targetIntent;
        RichFlyer.parseAction(targetIntent, new RFActionListener() {
            @Override
            public void onRFEventOnClickButton(@NonNull RFAction action, @NonNull String index) {
                try {
                    JSONObject json = new JSONObject();
                    json.put("Title", action.actionTitle);
                    json.put("Type", action.actionType);
                    json.put("Value", action.actionValue);
                    json.put("Index", parseButtonIndex(index));
                    UnityPlayer.UnitySendMessage(callbackObject, "onRichFlyerAction", json.toString());
                } catch (Exception e) {
                    android.util.Log.e("RichFlyer", "Failed to send an action to Unity.", e);
                }
            }

            @Override
            public void onRFEventOnClickStartApplication(String notificationId, String extendedProperty, @NonNull String index) {
                UnityPlayer.UnitySendMessage(callbackObject, "onRichFlyerExtendedProperty", extendedProperty == null ? "" : extendedProperty);
            }
        });
    }

    // RFActionParser passes the tapped button as "NotifyAction" + a 1-based button
    // number (e.g. "NotifyAction1"), not a bare integer, so it can't be Integer.parseInt'd directly.
    private static int parseButtonIndex(String notifyAction) {
        String digits = notifyAction.replaceAll("\\D+", "");
        return digits.isEmpty() ? 0 : Integer.parseInt(digits);
    }
}
