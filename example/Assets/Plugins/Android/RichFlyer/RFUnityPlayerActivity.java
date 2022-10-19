package jp.co.infocity.richflyer;

import android.content.Context;
import android.util.Log;
import android.os.Bundle;
import android.content.Intent;
import android.content.SharedPreferences;

import androidx.annotation.NonNull;

import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import jp.co.infocity.richflyer.action.RFAction;
import jp.co.infocity.richflyer.action.RFActionListener;

public class RFUnityPlayerActivity extends com.google.firebase.MessagingUnityPlayerActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onResume() {
        super.onResume();
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);

        sendRichFlyerActionEvent(intent);
    }

    private void sendRichFlyerActionEvent(Intent intent) {
        SharedPreferences pref = getApplicationContext().getSharedPreferences("jp.co.infocity.richflyer.preferences", Context.MODE_PRIVATE);
        String callbackObject = pref.getString("RFContentCallbackTargetObject", "");
        if (callbackObject.isEmpty()) {
            return;
        }

        if (RichFlyer.richFlyerAction(getIntent())) {
            RichFlyer.parseAction(getIntent(), new RFActionListener() {
                @Override
                public void onRFEventOnClickButton(@NonNull RFAction action, @NonNull String index) {
                    try {
                        JSONObject json = new JSONObject();
                        json.put("Title", action.actionTitle);
                        json.put("Type", action.actionType);
                        json.put("Value", action.actionValue);
                        json.put("Index", index);
                        UnityPlayer.UnitySendMessage(callbackObject, "onRichFlyerAction", json.toString());
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                }

                @Override
                public void onRFEventOnClickStartApplication(String notificationId, String extendedProperty, @NonNull String index) {
                    UnityPlayer.UnitySendMessage(callbackObject, "onRichFlyerExtendedProperty", extendedProperty==null?"":extendedProperty);
                }
            });
        }
    }
}
