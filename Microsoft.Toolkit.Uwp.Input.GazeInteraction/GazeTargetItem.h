//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

#pragma once

#include "DwellProgressState.h"
#include "GazeInput.h"
#include "PointerState.h"

using namespace Windows::Foundation;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls::Primitives;

BEGIN_NAMESPACE_GAZE_INPUT

private ref class GazeTargetItem abstract
{
internal:
    property TimeSpan DetailedTime;
    property TimeSpan OverflowTime;
    property TimeSpan ElapsedTime { TimeSpan get() { return DetailedTime + OverflowTime; } }
    property TimeSpan NextStateTime;
    property TimeSpan LastTimestamp;
    property PointerState ElementState;
    property int RepeatCount;
    property int MaxDwellRepeatCount;

    GazeTargetItem(UIElement^ target)
    {
    }

    static GazeTargetItem^ GetOrCreate(UIElement^ element);

    virtual void Invoke(UIElement^ element) = 0;

    virtual property bool IsInvokable { bool get() { return true; } }

    void Reset(UIElement^ element, TimeSpan nextStateTime)
    {
        ElementState = PointerState::PreEnter;
        DetailedTime = TimeSpanZero;
        OverflowTime = TimeSpanZero;
        NextStateTime = nextStateTime;
        RepeatCount = 0;
        MaxDwellRepeatCount = GazeInput::GetMaxDwellRepeatCount(element);
    }

    void GiveFeedback(UIElement^ element)
    {
        if (_nextStateTime != NextStateTime)
        {
            _prevStateTime = _nextStateTime;
            _nextStateTime = NextStateTime;
        }

        if (ElementState != _notifiedPointerState)
        {
            switch (ElementState)
            {
            case PointerState::Enter:
                RaiseProgressEvent(element, DwellProgressState::Fixating);
                break;

            case PointerState::Dwell:
            case PointerState::Fixation:
                RaiseProgressEvent(element, DwellProgressState::Progressing);
                break;

            case PointerState::Exit:
            case PointerState::PreEnter:
                RaiseProgressEvent(element, DwellProgressState::Idle);
                break;
            }

            _notifiedPointerState = ElementState;
        }
        else if (ElementState == PointerState::Dwell || ElementState == PointerState::Fixation)
        {
            if (RepeatCount <= MaxDwellRepeatCount)
            {
                RaiseProgressEvent(element, DwellProgressState::Progressing);
            }
            else
            {
                RaiseProgressEvent(element, DwellProgressState::Complete);
            }
        }
    }

private:

    void RaiseProgressEvent(UIElement^ element, DwellProgressState state);

    PointerState _notifiedPointerState = PointerState::Exit;
    TimeSpan _prevStateTime;
    TimeSpan _nextStateTime;
    DwellProgressState _notifiedProgressState = DwellProgressState::Idle;
    Popup^ _feedbackPopup;
};

END_NAMESPACE_GAZE_INPUT