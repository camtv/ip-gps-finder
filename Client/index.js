import { LitElement, html, css, customElement, property } from 'lit-element';
import "index.scss";

export class MapComponent extends LitElement {

    static get properties() {
        return {
            prop: { type: String },
        };
    }

    constructor() {
        super();

    }

    createRenderRoot() {
        return this;
    }

    render() {
            return html`                
                <div>
                    Componente mappa                    
                </div>  
            `;
    }

    firstUpdated() {

    }

    updated(changedProperties) {

	}

}

customElements.define('map-component', MapComponent);