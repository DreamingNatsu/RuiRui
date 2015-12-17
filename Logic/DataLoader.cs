using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Dba.DAL;
using Dba.DTO;

namespace Logic
{
    public class DataLoader
    {
        public int LoadCsv()
        {
            var data = GetDataFromCsvFile("Content\\default.csv", "category\tname\turl\timg\thtml");
            var categories = new List<Category>();
            foreach (var line in data)
            {
                if (!categories.Exists(d => d.Name == line[0]))
                {
                    categories.Add(new Category(){Name = line[0],Items = new List<Item>()});
                }
                var firstOrDefault = categories.FirstOrDefault(d => d.Name == line[0]);
                if (firstOrDefault != null)
                    firstOrDefault.Items.Add(new Item(){Name = line[1],Url = line[2],Image = line[3],CustomHTML = line[4]});
            }


            var urlList = new UrlList {Name = "Default", Categories = categories};
            using (var db = new DbCtx())
            {
                foreach (var category in categories)
                {
                    foreach (var item in category.Items)
                    {
                        db.Items.Add(item);
                        db.SaveChanges();
                    }
                    db.Categories.Add(category);
                    db.SaveChanges();
                }

                db.UrlLists.Add(urlList);
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    Console.WriteLine(e.Data);
                }


                db.Styles.Add(new Style()
                {
                    CSS = "@font-face {    font-family:\"Squada One\";    font-style:normal;    font-weight:400;src:local(\"Squada One\"), local(\"SquadaOne-Regular\"), url(\"../fonts/SquadaOne.woff\") format(\"woff\");}@-moz-keyframes animate-panel {    0% { transform:rotateX(90deg); }    100% { transform:rotateX(0deg);}}body, html {	background-color: #1d1d1d;    }#wrapper {    position:absolute;    margin-top:-450px;    margin-left:-400px;    top:50%;    left:50%;}#sites {    width:800px;    height: 900px;    position: relative;    margin-top:-40px;    margin-bottom: -40px;    padding:0px 10px 5px ;    text-align:center;    -webkit-animation: animate-sites 1s cubic-bezier(0.3, .5, .5, 0.6);    -moz-animation: animate-sites 2s cubic-bezier(0.3, .5, .5, 0.6);    -webkit-transition: all .4s cubic-bezier(0.75, -0.5, 0.25, 1.5) .1s;    -moz-transition: all .4s /*cubic-bezier(.75,0,.25,1) .1s*/;}#form{    background:rgba(22,22,22,.7);    border:1px solid rgba(30,30,30,.6);    border-bottom:1px solid rgba(48,48,48,.8);    color:#9f0000;    font-family:'Squada One';    font-size:275%;    height:125px;    width:auto;    line-height:125px;    margin:5px;    border-radius:5px;    box-shadow:inset #0f0f0f 0 -1px 6px;    -webkit-transition: all .4s cubic-bezier(0.75, -0.5, 0.25, 1.5) .1s;    -moz-transition: all .4s /*cubic-bezier(.75,0,.25,1) .1s*/;    -webkit-animation: animate-sites .5s cubic-bezier(0.3, .5, .5, 0.6);    -moz-animation: animate-sites .5s cubic-bezier(0.3, .5, .5, 0.6);    }.anim{        color:rgb(230, 230, 230);    font-family:'Squada One';    text-decoration:none;    text-shadow:#0f0f0f  0px 0px 5px;            -webkit-transition: all .4s cubic-bezier(0.75, -0.5, 0.25, 1.5) .1s;    -moz-transition: all .4s /*cubic-bezier(.75,0,.25,1) .1s*/;}.anim:hover, .anim:active {    color:rgb(230, 230, 230);    transform:scale(0.95);    -webkit-transform:scale(0.95);    -moz-transform:scale(0.95);    text-shadow:#060606 0px 0px 5px, #060606 0px 1px, #1a1616 0px 2px;        color: white;        }.anim:active{    font-size: 95% !important;}.hide > span{    color:hsla(0, 0%, 100%, 0)!important;    text-shadow:none!important;    -webkit-transition: all .4s cubic-bezier(0.75, -0.5, 0.25, 1.5) .1s;    -moz-transition: all .4s /*cubic-bezier(.75,0,.25,1) .1s*/;    -webkit-animation: animate-sites .5s cubic-bezier(0.3, .5, .5, 0.6);    -moz-animation: animate-sites .5s cubic-bezier(0.3, .5, .5, 0.6);}.hide:active > span, .hide:hover > span{    color:rgb(230, 230, 230)!important;    transform:scale(0.95)!important;    -webkit-transform:scale(0.95)!important;    -moz-transform:scale(0.95)!important;        text-shadow:#060606 0px 0px 5px, #060606 0px 1px, #1a1616 0px 2px !important;    }#search {        z-index:1;}#search form {        position:relative;            -webkit-animation: animate-opacity .4s ease-in;    -moz-animation: animate-opacity .4s ease-in;}#search input{        background:rgba(22,22,22,.7);    border:1px solid rgba(30,30,30,.6);    border-bottom:1px solid rgba(48,48,48,.8);        width:144px;    margin-bottom:25px;    padding:7px;    border-radius:5px;    box-shadow:inset #0f0f0f 0 0 2px;    color: white;    }#search input[type=\"text\"]:hover, #search input[type=\"text\"]:focus {    box-shadow:inset #0f0f0f 0 0 6px;}.empty{    height: 0!important;}.hexrow {    white-space: nowrap;    /*right/left margin set at (( width of child div x sin(30) ) / 2)     makes a fairly tight fit;     a 3px bottom seems to match*/    margin: 0 0px 5px;}.hexrow > a {    width: 100px;    height: 173.2px; /* ( width x cos(30) ) x 2 */    /* For margin:    right/left = ( width x sin(30) ) makes no overlap    right/left = (( width x sin(30) ) / 2) leaves a narrow separation    */    margin: 0 27px;    position: relative;    background-position: -50px 0; /* -left position -1 x width x sin(30) */    background-repeat: no-repeat;    color: #ffffff;    text-align: center;    line-height: 173.2px; /*equals height*/    display: inline-block;    z-index: 3;}.hexrow > a:nth-child(odd) {    top: 43.3px; /* ( width x cos(30) / 2 ) */}.hexrow > a:nth-child(even) {    top: -44.8px; /* -1 x( ( width x cos(30) / 2) + (hexrow bottom margin / 2)) */}.hexrow > a > div:first-of-type {    position: absolute;    width: 100%;    height: 100%;    top: 0;    left: 0;    z-index: -1;    overflow: hidden;    background-image: inherit;    -ms-transform:rotate(60deg); /* IE 9 */    -moz-transform:rotate(60deg); /* Firefox */    -webkit-transform:rotate(60deg); /* Safari and Chrome */    -o-transform:rotate(60deg); /* Opera */    transform:rotate(60deg);}.hexrow > a > div:first-of-type:before {    content: '';    position: absolute;    width: 200px; /* width of main + margin sizing */    height: 100%;    background-image: inherit;    background-position: top left;    background-repeat: no-repeat;    bottom: 0;    left: 0;    z-index: 1;    -ms-transform:rotate(-60deg) translate(-150px, 0); /* IE 9 */    -moz-transform:rotate(-60deg) translate(-150px, 0); /* Firefox */    -webkit-transform:rotate(-60deg) translate(-150px, 0); /* Safari and Chrome */    -o-transform:rotate(-60deg) translate(-150px, 0); /* Opera */    transform:rotate(-60deg) translate(-150px, 0);    -ms-transform-origin: 0 0; /* IE 9 */    -webkit-transform-origin: 0 0; /* Safari and Chrome */    -moz-transform-origin: 0 0; /* Firefox */    -o-transform-origin: 0 0; /* Opera */    transform-origin: 0 0;}.hexrow > a > div:last-of-type {    content: '';    position: absolute;    width: 100%;    height: 100%;    top: 0;    left: 0;    z-index: -2;    overflow: hidden;    background-image: inherit;    -ms-transform:rotate(-60deg); /* IE 9 */    -moz-transform:rotate(-60deg); /* Firefox */    -webkit-transform:rotate(-60deg); /* Safari and Chrome */    -o-transform:rotate(-60deg); /* Opera */    transform:rotate(-60deg);}.hexrow > a > div:last-of-type:before {    content: '';    position: absolute;    width: 200px; /* starting width + margin sizing */    height: 100%;    background-image: inherit;    background-position: top left;    background-repeat: no-repeat;    bottom: 0;    left: 0;    z-index: 1;    /*translate properties are initial width (100px) and half height (173.2 / 2 = 86.6) */    -ms-transform:rotate(60deg) translate(100px, 86.6px); /* IE 9 */    -moz-transform:rotate(60deg) translate(100px, 86.6px); /* Firefox */    -webkit-transform:rotate(60deg) translate(100px, 86.6px); /* Safari and Chrome */    -o-transform:rotate(60deg) translate(100px, 86.6px); /* Opera */    transform:rotate(60deg) translate(100px, 86.6px);    -ms-transform-origin: 100% 0; /* IE 9 */    -webkit-transform-origin: 100% 0; /* Safari and Chrome */    -moz-transform-origin: 100% 0; /* Firefox */    -o-transform-origin: 100% 0; /* Opera */    transform-origin: 100% 0;}.hexrow > a > span {    font-size:275%;    color:rgb(230, 230, 230);     display: inline-block;    margin: 0 -30px;    line-height: 1;    vertical-align: middle;    white-space: normal;}",
                    CategoryHTML = "<div class=\"hexrow\">@&items&@</div>",
                    ItemHTML = "<a class=\"anim hide\" style=\"background-image: url(@&img&@)\" href=\"@&url&@\"><span>@&name&@</span><div></div><div></div></a>",
                    PageHTML = "<div id=\"wrapper\"><div id=\"sites\">@&body&@</div></div>"
                });
                db.SaveChanges();
            }





            return 0;
        }



        public List<string[]> GetDataFromCsvFile(string csvFilePath)
        {
            return File.ReadAllLines(csvFilePath, Encoding.Default).Select(t => t.Split('\t')).Where(a => a.Count() > 1).ToList();
        }

        //geeft files terug van de directory /Content/Data/*filename*
        public List<string[]> GetDataFromDataDir(string filename, string firstline)
        {
            filename = HttpContext.Current.Server.MapPath("~") + "Content\\Data\\" + filename;
            return GetDataFromCsvFile(filename, firstline);
        }

        public List<string[]> GetDataFromCsvFile(string csvFilePath, string firstline)
        {
            var file = File.ReadAllLines(csvFilePath, Encoding.Default);
            if (file.First() != firstline) throw new FormatException();
            return file.Skip(1).Select(t => t.Split('\t')).Where(a => a.Count() > 1).ToList();
        }
    }
}