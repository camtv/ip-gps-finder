import { LitElement, html, css, customElement, property } from 'lit-element';
import "./index.scss";

export class MapComponent extends LitElement {

    // TODO: 
    //  - Creare un componente mappa che prenda tutta l'area del contenitore e visualizzi una mappa di google
    //  - Usare gli oggetto mappa google o creare un componente "hotPoint" che visualizza sulla mappa un tag ad una locazione stabilita
    //  - Caricare gli hotPoint tramite chiamata alla Api Locations


    static get properties() {
        return {
            nome_proprieta: { type: String },
        };
    }

    constructor() {
        super();
        this.nome_proprieta = "";

    }

    createRenderRoot() {
        return this;
    }

    render() {
            return html`                
                <div>
                    Componente mappa  ${this.nome_proprieta}                   
                </div>  
            `;
    }

 //   firstUpdated() {

 //   }

 //   updated(changedProperties) {

	//}

}

customElements.define('map-component', MapComponent);