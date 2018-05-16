using System;
using System.Collections.Generic;
using WinCompData.Mgc;
using WinCompData.Sn;
using WinCompData.Tools;

namespace WinCompData.Mgcg
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CanvasPathBuilder : IDisposable
    {
        readonly ListOfNeverNull<Command> _commands = new ListOfNeverNull<Command>();

        public CanvasPathBuilder(CanvasDevice device) { }

        public void BeginFigure(Vector2 vector2)
        {
            _commands.Add(new Command { Type = CommandType.BeginFigure, Args = vector2 });
        }
        public void EndFigure(CanvasFigureLoop loop)
        {
            _commands.Add(new Command { Type = CommandType.EndFigure, Args = loop });
        }
        public void AddCubicBezier(Vector2 a, Vector2 b, Vector2 c)
        {
            _commands.Add(new Command{Type = CommandType.AddCubicBezier, Args = new[] { a, b, c }});
        }

        public void SetFilledRegionDetermination(CanvasFilledRegionDetermination a)
        {
            _commands.Add(new Command { Type = CommandType.SetFilledRegionDetermination, Args = a });
        }

        public IEnumerable<Command> Commands => _commands;

        public void Dispose()
        {
        }

        public struct Command
        {
            public CommandType Type;
            public object Args;
        }
        public enum CommandType
        {
            BeginFigure,
            EndFigure,
            AddCubicBezier,
            SetFilledRegionDetermination,
        }
    }
}
