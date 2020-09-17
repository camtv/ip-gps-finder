import { LitElement, html, css, customElement, property } from 'lit-element';
import "./index.scss";
import "google-maps-limited/google-maps-limited.js";

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
              this._putMarkersOnMap(this.locations);
        })

        this.locations  = [
            {
              position: {lat:46.067952, lng:13.235577},
              InfoWindowContent: "<h3>Udine</h3>",
              Description: "Udine"
            },
            {
                position: {lat:45.964525, lng:12.663983},
                InfoWindowContent: "<h3>Pordenone</h3>",
                Description: "Pordenone"
              }
          ]

    }

    firstUpdated() {
        this.shadowRoot.appendChild(this._mapScriptTag());
        super.firstUpdated();
    }

    _putMarkersOnMap(markers) {

        if(!this._mapRef || !markers) return;
        if(this._locations) this._locations.map((marker) => marker.setMap(null));
        this._locations = markers.reduce((acc, item, index) => {
          if(item.position){
            const mapMarker = new google.maps.Marker({
              position: item.position,
              icon: 'https://developers.google.com/maps/documentation/javascript/examples/full/images/info-i_maps.png',
              map: this._mapRef
            })
            mapMarker.addListener('click', () => {
              this.selectedMarkerId = item.id || index;
              alert(`${item.Description} : LAT ${item.position.lat} / LNG ${item.position.lng}`)
            });
            acc[ item.id || index ] = mapMarker;
            return acc;
          }
          return acc;
        }, []);
        this._setDefaultBounds ();
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