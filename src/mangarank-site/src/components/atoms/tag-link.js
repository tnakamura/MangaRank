import React from 'react'
import { Link } from 'gatsby'
import { Badge } from 'reactstrap'

const TagLink = ({ name }) => (
  <Badge color="info"
         tag={Link}
         to={`/items/tagged/${name}`}>
    {name}
  </Badge>
)

export default TagLink
