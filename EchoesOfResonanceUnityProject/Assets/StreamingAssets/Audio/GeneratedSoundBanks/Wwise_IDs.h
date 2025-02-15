/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID ACTIVATESTEPDUCKING = 3348497029U;
        static const AkUniqueID PAUSEALL = 4091047182U;
        static const AkUniqueID PLAYBROADCASTERBEEP = 2409822221U;
        static const AkUniqueID PLAYBROADCASTERBLOOP = 893306899U;
        static const AkUniqueID PLAYBROADCASTERFX = 1668433167U;
        static const AkUniqueID PLAYBROADCASTERNOTE = 3594363913U;
        static const AkUniqueID PLAYBROADCASTERPLUNK = 879710867U;
        static const AkUniqueID PLAYCHECKPOINTREACHED = 1481495309U;
        static const AkUniqueID PLAYDOOROPEN = 3639922799U;
        static const AkUniqueID PLAYELEVATORHINTS = 3253062429U;
        static const AkUniqueID PLAYENDCUTSCENE = 3310685126U;
        static const AkUniqueID PLAYFOOTSTEPS = 1088348632U;
        static const AkUniqueID PLAYINTROAMBIENCE = 3384925243U;
        static const AkUniqueID PLAYSHUTOFF = 1900847084U;
        static const AkUniqueID PLAYTORCHEXTINGUISH = 3679901891U;
        static const AkUniqueID PLAYTORCHIGNITE = 1562608897U;
        static const AkUniqueID RESUMEALL = 3240900869U;
        static const AkUniqueID STARTINTROBRIDGECHOIR = 1116304881U;
        static const AkUniqueID STARTMETRONOME01 = 3352024112U;
        static const AkUniqueID STARTMUSIC01 = 3518514029U;
        static const AkUniqueID STOPBROADCASTERFX = 2261249677U;
        static const AkUniqueID STOPINTROAMBIENCE = 941857173U;
        static const AkUniqueID STOPINTROBRIDGECHOIR = 2373524099U;
        static const AkUniqueID STOPTONE = 2019189489U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace ENTRANCEHALL
        {
            static const AkUniqueID GROUP = 270858878U;

            namespace STATE
            {
                static const AkUniqueID BASE = 1291433366U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID PUZZLE01 = 3267866964U;
                static const AkUniqueID PUZZLE02 = 3267866967U;
                static const AkUniqueID PUZZLE03 = 3267866966U;
                static const AkUniqueID PUZZLE04 = 3267866961U;
            } // namespace STATE
        } // namespace ENTRANCEHALL

        namespace LEVEL01MASTER
        {
            static const AkUniqueID GROUP = 3800814886U;

            namespace STATE
            {
                static const AkUniqueID CLIFFSEDGE = 1149990767U;
                static const AkUniqueID ELEVATORHALLWAY = 1379644315U;
                static const AkUniqueID ELEVATORSHAFT = 1318468365U;
                static const AkUniqueID ENTRANCEHALL = 270858878U;
                static const AkUniqueID INTRO = 1125500713U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace LEVEL01MASTER

    } // namespace STATES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID FIRSTELEVATOR_HEIGHT = 1867101979U;
        static const AkUniqueID INTROBRIDGE_CROSSBRIDGE = 2726482464U;
        static const AkUniqueID INTROBRIDGE_FADEIN = 3217007934U;
        static const AkUniqueID INTROBRIDGE_STEPDUCK = 1325667752U;
        static const AkUniqueID MIDI_PITCH = 831692701U;
        static const AkUniqueID MUSIC_VOLUME = 1006694123U;
        static const AkUniqueID SFX_VOLUME = 1564184899U;
    } // namespace GAME_PARAMETERS

    namespace TRIGGERS
    {
        static const AkUniqueID BRIDGECROSSING01 = 1580584759U;
        static const AkUniqueID BRIDGECROSSING02 = 1580584756U;
        static const AkUniqueID ENTRANCEPUZZLECOMPLETE01 = 4134073719U;
        static const AkUniqueID ENTRANCEPUZZLECOMPLETE02 = 4134073716U;
        static const AkUniqueID ENTRANCEPUZZLECOMPLETE03 = 4134073717U;
        static const AkUniqueID ENTRANCEPUZZLECOMPLETE04 = 4134073714U;
        static const AkUniqueID HINTDELAY = 2526236795U;
        static const AkUniqueID TOBB = 3276123882U;
        static const AkUniqueID TOG = 1080872029U;
    } // namespace TRIGGERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID MAIN = 3161908922U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID LISTENERBUS = 572671253U;
        static const AkUniqueID MUSICBUS = 2886307548U;
        static const AkUniqueID SFXBUS = 3803850708U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID BROADCASTERVERB = 1198315964U;
        static const AkUniqueID PLAYERVERB = 3736295661U;
        static const AkUniqueID PUZZLEVERB = 2413215442U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
