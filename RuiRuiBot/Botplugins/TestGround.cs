using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Extensions;

namespace RuiRuiBot.Botplugins
{
    internal interface IGun
    {
        IMagazine Magazine { get; set; }
    }
    internal interface IBullet
    {
        void Fire();
    }

    internal interface IMagazine
    {
        IBullet GetBullet();
    }

    internal class Hollowpoint : IBullet
    {
        public void Fire(){
            Console.WriteLine("DOIN SOME DAMAGE BOII");
        }
    }

    internal class CompositeMagazine : IMagazine {
        private readonly IMagazine[] _magazines;
        private int _cursor = 0;
        public CompositeMagazine(params IMagazine[] magazines){
            _magazines = magazines;
        }

        public IBullet GetBullet(){
            IBullet b = null;
            while (b==null) {
                if (_cursor >= _magazines.Length) _cursor = 0;
                b = _magazines[_cursor]?.GetBullet();
                _cursor++;
            }
            return b;
        }
    }

    internal class DrumMag<TBullet> : IMagazine where TBullet : IBullet, new() {
        readonly Stack<IBullet> _bullets;

        public DrumMag(int bulletamount){
            _bullets = new Stack<IBullet>();
            Enumerable.Range(1, bulletamount).ForEach(n=>_bullets.Push(new TBullet()));
        }

        public IBullet GetBullet(){
            return _bullets.Pop();
        }
    }

    internal class NormalGlock : IGun
    {
        public NormalGlock(){

        }

        public IMagazine Magazine { get; set; }
    }
}
