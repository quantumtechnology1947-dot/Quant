// Current Date & Time

var dayarray=new Array("Sun","Mon","Tue","Wed","Thu","Fri","Sat")
var montharray=new Array("Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec")

function getthedate(){
var mydate=new Date()
var year=mydate.getYear()
if (year < 1000)
year+=1900
var day=mydate.getDay()
var month=mydate.getMonth()
var daym=mydate.getDate()
if (daym<10)
daym="0"+daym
var hours=mydate.getHours()
var minutes=mydate.getMinutes()
var seconds=mydate.getSeconds()
var dn="AM"
if (hours>=12)
dn="PM"
if (hours>12){
hours=hours-12
}
if (hours==0)
hours=12
if (minutes<=9)
minutes="0"+minutes
if (seconds<=9)
seconds="0"+seconds
//change font size here
var cdate="<small><font color='#FFFFFF' size='3' face='Arial'><b>"+dayarray[day]+", "+montharray[month]+" "+daym+", "+year+"&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"+hours+":"+minutes+":"+seconds+" "+dn+"</b></font></small>"
if (document.all)
document.all.clock.innerHTML=cdate
else if (document.getElementById)
document.getElementById("clock").innerHTML=cdate
else
document.write(cdate)
}
if (!document.all&&!document.getElementById)
getthedate()
function goforit(){
if (document.all||document.getElementById)
setInterval("getthedate()",1000)
}

// Time Message

function TimeMsg(){
datetoday = new Date();
timenow=datetoday.getTime();
datetoday.setTime(timenow);
thehour = datetoday.getHours();
if (thehour > 18) display = "Evening";
else if (thehour >12) display = "Afternoon";
else display = "Morning";
var greeting = ("Good " + display + "!");
document.write(greeting);
}

//Specify speed of scroll. Larger=faster (ie: 5)
var scrollspeed=cache=2

//Specify intial delay before scroller starts scrolling (in miliseconds):
var initialdelay=0

function initializeScroller(){
dataobj=document.all? document.all.datacontainer : document.getElementById("datacontainer")
dataobj.style.top="5px"
setTimeout("getdataheight()", initialdelay)
}

function getdataheight(){
thelength=dataobj.offsetHeight
if (thelength==0)
setTimeout("getdataheight()",5)
else
scrollDiv()
}

function scrollDiv(){
dataobj.style.top=parseInt(dataobj.style.top)-scrollspeed+"px"
if (parseInt(dataobj.style.top)<thelength*(-1))
dataobj.style.top="5px"
setTimeout("scrollDiv()",30)
}

if (window.addEventListener)
window.addEventListener("load", initializeScroller, false)
else if (window.attachEvent)
window.attachEvent("onload", initializeScroller)
else
window.onload=initializeScroller
