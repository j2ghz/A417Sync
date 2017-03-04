console.log("Modpacks script");

//import $ from "jquery";

function allowDrop(ev: Event) {
    ev.preventDefault();
}

function drag(ev: DragEvent) {
    var target = <HTMLElement>event.target;
    ev.dataTransfer.setData("text", target.id);
}

function drop(ev: DragEvent) {
    ev.preventDefault();
    //var data = ev.dataTransfer.getData("text");
    //var target = <HTMLElement>event.target;
    //var elementChildren = target.children;
    //var newElement = document.getElementById(data);
    //for (var i = 0; i < elementChildren.length; i++) {
    //    if (elementChildren[i].id == ) {
    //        return;
    //    }
    //}
    //TODO use jquery to get ul parent, look at html when dropping  if not sure
    //target.appendChild(newElement.cloneNode());
}