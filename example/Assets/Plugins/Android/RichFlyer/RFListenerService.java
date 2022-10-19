package jp.co.infocity.richflyer;

import android.util.Log;
import com.google.firebase.messaging.cpp.ListenerService;
import com.google.firebase.messaging.RemoteMessage;
import jp.co.infocity.richflyer.RFSendPushInformation;
import jp.co.infocity.richflyer.RichFlyer;

public class RFListenerService extends ListenerService {
    @Override
    public void onMessageReceived(RemoteMessage message) {        
        int resId = getApplicationContext().getResources().getIdentifier("ic_notification", "mipmap", getPackageName());

        //受信した情報をライブラリ側に渡す
        RFSendPushInformation spi = new RFSendPushInformation(this, resId);

        spi.setPushData(message.getData());

        super.onMessageReceived(message);
    }
}
