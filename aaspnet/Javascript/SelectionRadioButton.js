function SelectSingleRadiobutton(rdbtnid) {
var rdBtn = document.getElementById(rdbtnid);
var rdBtnList = document.getElementsByTagName("input");
for (i = 0; i < rdBtnList.length; i++) {
if (rdBtnList[i].type == "radio" && rdBtnList[i].id != rdBtn.id)
{
rdBtnList[i].checked = false;
}
}
}
