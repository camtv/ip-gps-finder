import { LitElement, html, css, customElement, property } from 'lit-element';
import "./index.scss";

window.initMap = function () { window.dispatchEvent(new CustomEvent('google-map-ready')); };

export class MapComponent extends LitElement {

    // TODO: 
    //  - Creare un componente mappa che prenda tutta l'area del contenitore e visualizzi una mappa di google
    //  - Usare gli oggetto mappa google o creare un componente "hotPoint" che visualizza sulla mappa un tag ad una locazione stabilita
    //  - Caricare gli hotPoint tramite chiamata alla Api Locations

    static get properties() {
        return {
            locations: { type: Array },
        };
    }

    constructor() {
        super();

        window.addEventListener('google-map-ready', () => {
            this._mapRef = new google.maps.Map(this.shadowRoot.querySelector('#map'), {
                zoom: 13,
                streetViewControl: false,
              });
              this._drawCustomMarkers(this.locations);
        })

        this.locations  = []
    }

    firstUpdated() {
        this.shadowRoot.appendChild(this._mapScriptTag());
        super.firstUpdated();
        fetch('http://127.0.0.1:3000')
			.then((res) => {
				return res.json();
			})
			.then((res) => {
				this.locations = res.map(x => ({
                    position: {lat:x.Latitude, lng:x.Longitude},
                    InfoWindowContent: `<h3>${x.Description}</h3>`,
                    Name: x.Description,
                    Description: "Descrizione della città",
                    Url: `http://www.${x.Description}.it`
                }));
			});
    }

    _setDefaultBounds () {
    if (this.locations.length === 0) {
        var worldBounds = new google.maps.LatLngBounds(
        new google.maps.LatLng(70.4043, -143.5291),
        new google.maps.LatLng(-46.11251, 163.4288)
        );
        this._mapRef.fitBounds(worldBounds, 0);
    } else {
        var initialBounds = this.locations.reduce((bounds, marker) => {
        bounds.extend(new google.maps.LatLng(marker.position.lat, marker.position.lng));
        return bounds;
        }, new google.maps.LatLngBounds());
        this._mapRef.fitBounds(initialBounds);
    }
    }

    _mapScriptTag() {
        const googleMapsLoader = document.createElement('script');
        googleMapsLoader.src = `https://maps.googleapis.com/maps/api/js?key=AIzaSyB-0ChznkjSGrML4dXud1Z6uv0Hdi6IlF4&callback=initMap`;
        googleMapsLoader.async = true;
        googleMapsLoader.defer = true;
        return googleMapsLoader;
    }

    _drawCustomMarkers(locations) {
        
        function addCustomMarker(map, location) {
            
            function HTMLMarker(location) {
                this.url = location.Url;
                this.name = location.Name;
                this.description = location.Description;
                this.pos = new google.maps.LatLng(location.position.lat, location.position.lng);
            }
    
            HTMLMarker.prototype = new google.maps.OverlayView();

            HTMLMarker.prototype.onRemove = function () {}

            HTMLMarker.prototype.onAdd = function () {
                this.div = document.createElement("div");
                this.div.style.position = "absolute";
                this.div.className = "htmlMarker";
                this.div.data = "data-price";
                this.image = document.createElement("img");
                this.image.src = "https://miro.medium.com/max/625/0*-ouKIOsDCzVCTjK-.png"
                this.div.appendChild(this.image);
                this.nameSpan = document.createElement("span");
                this.nameSpan.innerHTML = this.name;
                this.div.appendChild(this.nameSpan);
                this.descSpan = document.createElement("span");
                this.descSpan.innerHTML = this.description;
                this.div.appendChild(this.descSpan);
                this.link = document.createElement("a");
                this.link.href = this.url; 
                this.link.text = this.name;
                this.div.appendChild(this.link);
                var panes = this.getPanes();
                panes.overlayImage.appendChild(this.div);
            }
    
            HTMLMarker.prototype.draw = function () {
                var overlayProjection = this.getProjection();
                var position = overlayProjection.fromLatLngToDivPixel(this.pos);
                this.div.style.left = position.x - 60 + 'px';
                this.div.style.top = position.y - 80 + 'px';
            }
    
            var htmlMarker = new HTMLMarker(location);
    
            htmlMarker.setMap(map);
        }

        for (var i = 0; i < locations.length; i++) {
            addCustomMarker(this._mapRef,locations[i]);
        }

        this._setDefaultBounds ();
    }

    static get styles() {
        return css`
            :host {
                display: block
            }
            body {
                padding:0;
                margin:0;
            }
            #map {
                width: 100wv;
                height: 100vh;
            }
            .htmlMarker {
                width:120px;
                background-color:#ffffff;
                padding:2px 5px;
                border-radius: 2px;
                border: 1px solid #cccccc;
            }

            .htmlMarker a, .htmlMarker span {
                display:block;
            }

            .htmlMarker img {
                width:30px;
                height: auto;
                clear:both;
            }
        `
    }

    render() {
        const {  } = this;
            return html`          
            <div id="map"></div>
            `;
    }

}

customElements.define('map-component', MapComponent);