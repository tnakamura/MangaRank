import React from 'react'
import { Link } from 'gatsby'
import Img from 'gatsby-image'
import { Row, Col } from 'reactstrap'
import PenIcon from '../atoms/pen-icon'
import TagsIcon from '../atoms/tags-icon'
import BuildingIcon from '../atoms/building-icon'
import AmazonIcon from '../atoms/amazon-icon'

const ItemCell = ({ item, rank, rowClassName }) => (
  <Row className={rowClassName}>
    <Col md={2} sm={12}>
      <Img fixed={{
             width: 110,
             height: 160,
             src: item.imageUrl,
             srcSet: item.imageUrl
           }}
           alt={item.title}/>
    </Col>

    <Col md={10} sm={12}>
      <h4>
        <Link to={`/items/${item.asin}`}
              style={{color: `#337ab7`}}>
          {rank}. {item.title}
        </Link>
      </h4>

      <ul className="list-unstyled" 
          style={{color: `#666`, fontSize: `0.9em`}}>
        <li>
          <PenIcon title="著者"/>
          {item.author}
        </li>
        <li>
          <BuildingIcon title="出版社"/>
          {item.publisher}
        </li>
        <li>
          <TagsIcon title="タグ"/>
          {
            item.tags.map(t => 
              <Link style={{color: `#666`}}
                    className="ml-1"
                    key={`${item.id}_${t.name}`}
                    to={`/items/tagged/${t.name}`}>
                {t.name}
              </Link>
            )
          }
        </li>
        <li>
          <AmazonIcon title="Amazon"/>
          <a style={{color: `#666`}}
             className="ml-1"
             href={item.detailPageUrl}
             target="_blank"
             rel="noopener noreferrer">
            Amazon で購入
          </a>
        </li>
      </ul>
    </Col>
  </Row>
)

export default ItemCell
